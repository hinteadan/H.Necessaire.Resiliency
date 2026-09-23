using H.Necessaire.Resiliency.Abstractions;
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
            var val = TimeSpan.FromSeconds(7).EnsureMinMax(TimeSpan.Zero, true, TimeSpan.FromSeconds(10), true, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10));
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
