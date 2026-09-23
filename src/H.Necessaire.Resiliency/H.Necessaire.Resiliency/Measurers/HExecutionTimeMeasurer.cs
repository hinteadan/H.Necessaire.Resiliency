using H.Necessaire.Resiliency.Abstractions;
using H.Necessaire.Resiliency.Abstractions.DataModels;
using H.Necessaire.Resiliency.DataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Measurers
{
    public class HExecutionTimeMeasurer : ImAnHResiliencyMeasurer
    {
        const string contextKeyExecutionLogic = "HExecutionTime::ExecutionLogic";
        const string contextKeyExecutionTimeout = "HExecutionTime::ExecutionTimeout";
        static readonly TimeSpan defaultExecutionTimeout = TimeSpan.FromSeconds(30);
        static readonly TimeSpan maxExecutionTimeout = TimeSpan.FromMinutes(5);

        public async Task<OperationResult<ImAnHResiliencyMeasurement>> MeasureResiliency(ImAnHResiliencyMeasurementContext measurementContext)
        {
            if (measurementContext.IsEmpty())
                return "Measurement context is empty";

            return (await HSafe.Run<OperationResult<ImAnHResiliencyMeasurement>>(async () =>
            {

                ReadExecutionParamsFromContext(measurementContext, out var executionLogic, out var executionTimeout);

                if (executionLogic is null)
                    return $"Execution logic to be measured is not defined. It must be defined in the context, as Func<CancellationToken, Task> under the key: {contextKeyExecutionLogic}";

                using (var timeoutCts = new CancellationTokenSource(executionTimeout))
                {
                    OperationResult<TimeSpan?> executionResult = "Not yet started";
                    Task executionTask = Task.Run(async () =>
                    {
                        executionResult = await HSafe.Run(async () =>
                        {
                            TimeSpan? executionDuration = null;
                            using (new PreciseTimeMeasurement(x => executionDuration = x))
                            {
                                await executionLogic.Invoke(timeoutCts.Token);
                            }
                            return executionDuration;
                        });
                    });
                    Task timeoutTask = HSafe.Run(async () => await Task.Delay(Timeout.Infinite, timeoutCts.Token));
                    Task completedTask = await Task.WhenAny(executionTask, timeoutTask);

                    OperationResult<TimeSpan?> measurementResult
                        = completedTask == timeoutTask
                        ? $"Execution timed out after {executionTimeout}"
                        : executionResult
                        ;

                    return CalculateResiliencyMeasurement(measurementContext, measurementResult, executionTimeout);
                }

            }))
            .UnwrapToFirstFailOrLastWin()
            ;
        }

        static OperationResult<ImAnHResiliencyMeasurement> CalculateResiliencyMeasurement(ImAnHResiliencyMeasurementContext measurementContext, OperationResult<TimeSpan?> measurementResult, TimeSpan executionTimeout)
        {
            if (!measurementResult || measurementResult.Payload == null)
                return new HResiliencyMeasurement(measurementContext.ID, -1);

            NumberInterval executionInterval = (0, executionTimeout.Ticks);
            double myriadValue = new DataNormalizer(executionInterval, HResiliencyMeasurement.Myriad).Do(measurementResult.Payload.Value.Ticks);
            double flippedMyriadValue = myriadValue.FlipIntervalValueToOppositeEnd(HResiliencyMeasurement.Myriad);

            short score = (short)Math.Ceiling(flippedMyriadValue);

            return new HResiliencyMeasurement(measurementContext.ID, score, new Note[] { measurementResult.Payload.ToString().NoteAs("ExecutionDuration") });
        }

        static void ReadExecutionParamsFromContext(ImAnHResiliencyMeasurementContext measurementContext, out Func<CancellationToken, Task> executionLogic, out TimeSpan executionTimeout)
        {
            executionLogic = measurementContext.GetValueOrDefaultFor<Func<CancellationToken, Task>>(contextKeyExecutionLogic);

            executionTimeout 
                = (measurementContext.GetValueOrDefaultFor<TimeSpan?>(contextKeyExecutionTimeout) ?? defaultExecutionTimeout)
                .EnsureMinMax(TimeSpan.Zero, maxExecutionTimeout, valueIfLessThanMin: defaultExecutionTimeout)
                ;
        }

        public static ImAnHResiliencyMeasurementContext NewContext(string id, Func<CancellationToken, Task> executionLogic, TimeSpan? executionTimeout = null)
        {
            return new HResiliencyMeasurementContext(new Dictionary<string, object>() {
                { contextKeyExecutionLogic, executionLogic },
                { contextKeyExecutionTimeout, executionTimeout },
            })
            { ID = id };
        }
    }
}
