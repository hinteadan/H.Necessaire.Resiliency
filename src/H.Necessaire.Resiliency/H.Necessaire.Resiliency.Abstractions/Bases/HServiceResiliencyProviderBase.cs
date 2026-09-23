using H.Necessaire.Resiliency.Abstractions.DataModels;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Abstractions.Bases
{
    public abstract class HServiceResiliencyProviderBase<TService> : ImAnHServiceResiliencyProvider<TService>, ImADependency
    {
        readonly Func<ImADependencyProvider, TService> instanceFactory;

        public abstract string ID { get; }
        public abstract Task<OperationResult<ImAnHResiliencyMeasurementContext>> GetLatestResiliencyMeasurementContext();

        protected HServiceResiliencyProviderBase(Func<ImADependencyProvider, TService> instanceFactory)
        {
            this.instanceFactory = instanceFactory;
        }
        protected HServiceResiliencyProviderBase() : this(x => x.Get<TService>()) { }

        Func<TService> instanceProvider;
        public virtual void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            instanceProvider = () => instanceFactory == null ? default : instanceFactory(dependencyProvider);
        }

        public TService GetServiceInstance() => instanceProvider == null ? default : instanceProvider();
    }
}
