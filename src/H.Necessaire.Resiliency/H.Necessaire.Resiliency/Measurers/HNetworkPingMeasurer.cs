using H.Necessaire.Resiliency.Abstractions;
using H.Necessaire.Resiliency.Abstractions.DataModels;
using H.Necessaire.Resiliency.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Measurers
{
    public class HNetworkPingMeasurer : ImAnHResiliencyMeasurer
    {
        const string contextKeyHost = "HNetworkPing::Host";
        const string contextKeyTotalNumberOfPingsToPerform = "HNetworkPing::TotalNumberOfPingsToPerform";
        const string contextKeyTimeoutPerPing = "HNetworkPing::TimeoutPerPing";
        const string contextKeyTimeoutPerTotalPings = "HNetworkPing::TimeoutPerTotalPings";

        const int defaultTotalNumberOfPingsToPerform = 5;
        static readonly TimeSpan defaultTimeoutPerPing = TimeSpan.FromSeconds(3.14);
        static readonly TimeSpan maxTimeoutPerPing = TimeSpan.FromSeconds(17);
        static readonly TimeSpan defaultTimeoutPerTotalPings = TimeSpan.FromSeconds(3.14 * defaultTotalNumberOfPingsToPerform);
        static readonly TimeSpan maxTimeoutPerTotalPings = TimeSpan.FromSeconds(60);

        public async Task<OperationResult<ImAnHResiliencyMeasurement>> MeasureResiliency(ImAnHResiliencyMeasurementContext measurementContext)
        {
            if (measurementContext.IsEmpty())
                return "Measurement context is empty";

            return (await HSafe.Run<OperationResult<ImAnHResiliencyMeasurement>>(async () =>
            {

                if (!measurementContext.GetValueFor<string>(contextKeyHost).Ref(out var hostRes, out var host))
                    return hostRes.WithoutPayload<ImAnHResiliencyMeasurement>();

                if (host.IsEmpty())
                    return $"Host is empty, make sure you provide a valid ping host in the context via the key {contextKeyHost}";

                ReadPingParamsFromContext(measurementContext, out int totalNumberOfPingsToPerform, out TimeSpan timeoutPerPing, out TimeSpan timeoutPerTotalPings);

                int timeoutPerPingInMilliseconds = (int)Math.Ceiling(timeoutPerPing.TotalMilliseconds);

                List<OperationResult<long>> pingResultsWithTimeInMilliseconds = new List<OperationResult<long>>(totalNumberOfPingsToPerform);

                using (var ping = new Ping())
                using (var totalPingsTimeoutCts = new CancellationTokenSource(timeoutPerTotalPings))
                {
                    for (int pingIndex = 0; pingIndex < totalNumberOfPingsToPerform; pingIndex++)
                    {
                        pingResultsWithTimeInMilliseconds.Add(
                            (await HSafe.Run<OperationResult<long>>(async () =>
                            {
                                if (totalPingsTimeoutCts.IsCancellationRequested)
                                    return "Total pings timed out";

                                PingReply pingReply = await ping.SendPingAsync(host, timeoutPerPingInMilliseconds);
                                if (pingReply.Status != IPStatus.Success)
                                {
                                    return $"Ping failed: {pingReply.Status}";
                                }

                                return pingReply.RoundtripTime;
                            }))
                            .UnwrapToFirstFailOrLastWin()
                        );
                    }
                }

                return CalculatePingResiliencyMeasurement(measurementContext, pingResultsWithTimeInMilliseconds);

            })).UnwrapToFirstFailOrLastWin();
        }

        static OperationResult<ImAnHResiliencyMeasurement> CalculatePingResiliencyMeasurement(ImAnHResiliencyMeasurementContext measurementContext, IReadOnlyCollection<OperationResult<long>> pingResults)
        {
            if (pingResults.IsEmpty())
                return new HResiliencyMeasurement(measurementContext.ID, -1);

            int score = 10_000;
            int pingUnitScore = 10_000 / pingResults.Count;
            if (pingUnitScore == 0) pingUnitScore = 1;

            pingResults.ProcessStream(pingResult =>
            {

                if (!pingResult)
                {
                    score = score - pingUnitScore - 1;
                    return;
                }

                long pingTimeInMilliseconds = pingResult.Payload;

                if (pingTimeInMilliseconds <= 15)
                {
                    return;
                }
                else if (pingTimeInMilliseconds <= 50)
                {
                    score -= (pingUnitScore / 100).MorphIf(x => x == 0, _ => 1);
                    return;
                }
                else if (pingTimeInMilliseconds <= 100)
                {
                    score -= (pingUnitScore / 16).MorphIf(x => x == 0, _ => 2);
                    return;
                }
                else if (pingTimeInMilliseconds <= 150)
                {
                    score -= (pingUnitScore / 4).MorphIf(x => x == 0, _ => 3);
                    return;
                }
                else if (pingTimeInMilliseconds < 300)
                {
                    score -= (pingUnitScore / 2).MorphIf(x => x == 0, _ => 5);
                    return;
                }
                else
                {
                    score = score - pingUnitScore - 1;
                    return;
                }
            });

            return new HResiliencyMeasurement(measurementContext.ID, (short)score, pingResults.Select((r, i) => (r ? $"Successful, {r.Payload}ms" : $"Failed, {r.Reason}").NoteAs($"Ping{i}")).ToArray());
        }

        static void ReadPingParamsFromContext(ImAnHResiliencyMeasurementContext measurementContext, out int totalNumberOfPingsToPerform, out TimeSpan timeoutPerPing, out TimeSpan timeoutPerTotalPings)
        {
            totalNumberOfPingsToPerform 
                = (measurementContext.GetValueOrDefaultFor<int?>(contextKeyTotalNumberOfPingsToPerform) ?? defaultTotalNumberOfPingsToPerform)
                .EnsureMinMax(1, 17)
                ;

            timeoutPerPing 
                = (measurementContext.GetValueOrDefaultFor<TimeSpan?>(contextKeyTimeoutPerPing) ?? defaultTimeoutPerPing)
                .EnsureMinMax(TimeSpan.Zero, maxTimeoutPerPing, valueIfLessThanMin: defaultTimeoutPerPing)
                ;

            timeoutPerTotalPings
                = (measurementContext.GetValueOrDefaultFor<TimeSpan?>(contextKeyTimeoutPerTotalPings) ?? defaultTimeoutPerTotalPings)
                .EnsureMinMax(TimeSpan.Zero, maxTimeoutPerTotalPings, valueIfLessThanMin: defaultTimeoutPerTotalPings)
                ;
        }

        public static ImAnHResiliencyMeasurementContext NewContext(string id, string host, int? totalNumberOfPingsToPerform = null, TimeSpan? timeoutPerPing = null, TimeSpan? timeoutPerTotalPings = null)
        {
            return new HResiliencyMeasurementContext(new Dictionary<string, object>() {
                { contextKeyHost, host },
                { contextKeyTotalNumberOfPingsToPerform, totalNumberOfPingsToPerform },
                { contextKeyTimeoutPerPing, timeoutPerPing },
                { contextKeyTimeoutPerTotalPings, timeoutPerTotalPings },
            })
            { ID = id };
        }
    }
}
