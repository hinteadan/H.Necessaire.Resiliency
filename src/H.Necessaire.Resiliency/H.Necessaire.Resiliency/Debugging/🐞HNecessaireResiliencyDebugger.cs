using H.Necessaire.Resiliency.Abstractions;
using H.Necessaire.Resiliency.DataModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Debugging
{
    internal class HNecessaireResiliencyDebugger : ImADependency, IDebug
    {
        ImALogger log;
        ImACancellationManager cancellationManager;
        ImAnHResilientServiceProvider<ResiliencyTestService> resiliencyTestServiceProvider;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            log = dependencyProvider.GetLogger<HNecessaireResiliencyDebugger>();
            cancellationManager = dependencyProvider.Get<ImACancellationManager>();
            resiliencyTestServiceProvider = dependencyProvider.Get<ImAnHResilientServiceProvider<ResiliencyTestService>>();
        }

        public async Task Debug()
        {
            var x = (-5d).FlipIntervalValueToOppositeEnd(NumberInterval.Percent);

            var val = new double[] {
                1d.FlipIntervalValueToOppositeEnd(NumberInterval.Percent),
                99d.FlipIntervalValueToOppositeEnd(NumberInterval.Percent),
                150d.FlipIntervalValueToOppositeEnd(NumberInterval.Percent),
                100d.FlipIntervalValueToOppositeEnd(NumberInterval.Percent),
                (-5d).FlipIntervalValueToOppositeEnd(NumberInterval.Percent),
                1d.FlipIntervalValueToOppositeEnd(HResiliencyMeasurement.Myriad),
                5_000d.FlipIntervalValueToOppositeEnd(HResiliencyMeasurement.Myriad),
                2_000d.FlipIntervalValueToOppositeEnd(HResiliencyMeasurement.Myriad),
            };
            return;

            using (var _t = await log.LogInfoDuration(nameof(HNecessaireResiliencyDebugger), "🐞"))
            {
                (await resiliencyTestServiceProvider.GetAllServiceInstancesOrderedByResiliencyWithIDs()).Ref(out var res, out var services);

                ImAnHServiceResiliencyRunner<ResiliencyTestService> runner = await resiliencyTestServiceProvider.GetResiliencyRunner().ThrowOnFailOrReturn();

                OperationResult[] runResults = await Task.WhenAll(Enumerable.Range(0, 10).Select(i => runner.ResilientlyRun(s => s.RandomlyThrowError()).AsTask()));

                //ResiliencyTestService service = await resiliencyTestServiceProvider.GetBestKnownServiceInstance().ThrowOnFailOrReturn();
            }
        }
    }
}
