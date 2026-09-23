using H.Necessaire.Resiliency.Abstractions;
using H.Necessaire.Resiliency.Abstractions.DataModels;
using H.Necessaire.Resiliency.DataModels;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Debugging
{
    internal class TestServiceMeasurer : ImAnHResiliencyMeasurer
    {
        static readonly Random random = new Random();

        public Task<OperationResult<ImAnHResiliencyMeasurement>> MeasureResiliency(ImAnHResiliencyMeasurementContext measurementContext)
        {
            return (new HResiliencyMeasurement(measurementContext?.GetValueOrDefaultFor<string>("ID") ?? "N/A", (short)random.Next(0, short.MaxValue)) as ImAnHResiliencyMeasurement).ToWinResult().AsTask();
        }
    }
}
