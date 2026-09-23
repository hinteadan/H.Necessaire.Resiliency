using H.Necessaire.Resiliency.Abstractions;
using H.Necessaire.Resiliency.Abstractions.Bases;
using H.Necessaire.Resiliency.Measurers;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Debugging
{
    internal class ResiliencyTestServiceProvider : HResilientServiceProviderBase<ResiliencyTestService>
    {
        ImAnHServiceResiliencyProvider<ResiliencyTestService>[] resilientServices = null;
        ImAnHResiliencyMeasurer measurer = null;
        public override void ReferDependencies(ImADependencyProvider deps)
        {
            base.ReferDependencies(deps);

            Random random = new Random();

            resilientServices = new ImAnHServiceResiliencyProvider<ResiliencyTestService>[] {
                //"hintea.com".Morph(id => new HServiceResiliencyProvider<ResiliencyTestService>(id, x => new ResiliencyTestService(id), HNetworkPingMeasurer.NewContext(id, host: "hintea.com")).And(x => x.ReferDependencies(dependencyProvider))),
                //"google.com".Morph(id => new HServiceResiliencyProvider<ResiliencyTestService>(id, x => new ResiliencyTestService(id), HNetworkPingMeasurer.NewContext(id, host: "google.com")).And(x => x.ReferDependencies(dependencyProvider))),
                //"rovfr.com".Morph(id => new HServiceResiliencyProvider<ResiliencyTestService>(id, x => new ResiliencyTestService(id), HNetworkPingMeasurer.NewContext(id, host: "rovfr.com")).And(x => x.ReferDependencies(dependencyProvider))),
                //"onvfr.com".Morph(id => new HServiceResiliencyProvider<ResiliencyTestService>(id, x => new ResiliencyTestService(id), HNetworkPingMeasurer.NewContext(id, host: "onvfr.com")).And(x => x.ReferDependencies(dependencyProvider))),
                //"hintee.asuscomm.com".Morph(id => new HServiceResiliencyProvider<ResiliencyTestService>(id, x => new ResiliencyTestService(id), HNetworkPingMeasurer.NewContext(id, host: "hintee.asuscomm.com")).And(x => x.ReferDependencies(dependencyProvider))),
                "A".Morph(id => deps.NewHServiceResiliencyProvider(id, x => new ResiliencyTestService(id), HExecutionTimeMeasurer.NewContext(id, executionLogic: async ct => await Task.Delay(random.Next(100, 1000), ct), executionTimeout: TimeSpan.FromMilliseconds(1000)))),
                "B".Morph(id => deps.NewHServiceResiliencyProvider(id, x => new ResiliencyTestService(id), HExecutionTimeMeasurer.NewContext(id, executionLogic: async ct => await Task.Delay(random.Next(100, 1000), ct), executionTimeout: TimeSpan.FromMilliseconds(1000)))),
                "C".Morph(id => deps.NewHServiceResiliencyProvider(id, x => new ResiliencyTestService(id), HExecutionTimeMeasurer.NewContext(id, executionLogic: async ct => await Task.Delay(random.Next(100, 1000), ct), executionTimeout: TimeSpan.FromMilliseconds(1000)))),
            };

            //measurer = dependencyProvider.Get<HNetworkPingMeasurer>();
            measurer = deps.Get<HExecutionTimeMeasurer>();
        }

        protected override ImAnHServiceResiliencyProvider<ResiliencyTestService>[] ResilientServices => resilientServices;

        protected override ImAnHResiliencyMeasurer ResiliencyMeasurer => measurer;
    }
}
