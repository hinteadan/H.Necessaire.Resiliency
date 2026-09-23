using H.Necessaire.Resiliency.Abstractions.Bases;
using H.Necessaire.Resiliency.Abstractions.DataModels;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency
{
    internal class HServiceResiliencyProvider<TService> : HServiceResiliencyProviderBase<TService>
    {
        readonly string id;
        readonly Func<Task<OperationResult<ImAnHResiliencyMeasurementContext>>> contextProvider;


        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, Func<Task<OperationResult<ImAnHResiliencyMeasurementContext>>> contextProvider)
            : base(instanceFactory)
        {
            this.id = id;
            this.contextProvider = async () => (await HSafe.Run(contextProvider)).UnwrapToFirstFailOrLastWin();
        }
        public HServiceResiliencyProvider(string id, Func<Task<OperationResult<ImAnHResiliencyMeasurementContext>>> contextProvider)
        {
            this.id = id;
            this.contextProvider = async () => (await HSafe.Run(contextProvider)).UnwrapToFirstFailOrLastWin();
        }


        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, Func<Task<ImAnHResiliencyMeasurementContext>> contextProvider)
            : this(id, instanceFactory, async () => contextProvider is null ? "contextProvider is not specified" : await HSafe.Run(contextProvider, "contextProvider()"))
        { }
        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, Func<OperationResult<ImAnHResiliencyMeasurementContext>> contextProvider)
            : this(id, instanceFactory, () => (contextProvider is null ? "contextProvider is not specified" : contextProvider()).AsTask())
        { }
        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, Func<ImAnHResiliencyMeasurementContext> contextProvider)
            : this(id, instanceFactory, () => (contextProvider is null ? "contextProvider is not specified" : HSafe.Run(contextProvider, "contextProvider()")).AsTask())
        { }


        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, Task<OperationResult<ImAnHResiliencyMeasurementContext>> context)
            : this(id, instanceFactory, async () => context is null ? "context is not specified" : await context)
        { }
        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, Task<ImAnHResiliencyMeasurementContext> context)
            : this(id, instanceFactory, async () => context is null ? "context is not specified" : await HSafe.Run(async () => await context))
        { }
        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, OperationResult<ImAnHResiliencyMeasurementContext> context)
            : this(id, instanceFactory, () => (context is null ? "context is not specified" : context).AsTask())
        { }
        public HServiceResiliencyProvider(string id, Func<ImADependencyProvider, TService> instanceFactory, ImAnHResiliencyMeasurementContext context)
            : this(id, instanceFactory, () => (context is null ? "context is not specified" : context.ToWinResult()).AsTask())
        { }



        public HServiceResiliencyProvider(string id, Func<Task<ImAnHResiliencyMeasurementContext>> contextProvider)
            : this(id, async () => contextProvider is null ? "contextProvider is not specified" : await HSafe.Run(contextProvider, "contextProvider()"))
        { }
        public HServiceResiliencyProvider(string id, Func<OperationResult<ImAnHResiliencyMeasurementContext>> contextProvider)
            : this(id, () => (contextProvider is null ? "contextProvider is not specified" : contextProvider()).AsTask())
        { }
        public HServiceResiliencyProvider(string id, Func<ImAnHResiliencyMeasurementContext> contextProvider)
            : this(id, () => (contextProvider is null ? "contextProvider is not specified" : HSafe.Run(contextProvider, "contextProvider()")).AsTask())
        { }


        public HServiceResiliencyProvider(string id, Task<OperationResult<ImAnHResiliencyMeasurementContext>> context)
            : this(id, async () => context is null ? "context is not specified" : await context)
        { }
        public HServiceResiliencyProvider(string id, Task<ImAnHResiliencyMeasurementContext> context)
            : this(id, async () => context is null ? "context is not specified" : await HSafe.Run(async () => await context))
        { }
        public HServiceResiliencyProvider(string id, OperationResult<ImAnHResiliencyMeasurementContext> context)
            : this(id, () => (context is null ? "context is not specified" : context).AsTask())
        { }
        public HServiceResiliencyProvider(string id, ImAnHResiliencyMeasurementContext context)
            : this(id, () => (context is null ? "context is not specified" : context.ToWinResult()).AsTask())
        { }


        public override string ID => id;

        public override async Task<OperationResult<ImAnHResiliencyMeasurementContext>> GetLatestResiliencyMeasurementContext()
        {
            if (contextProvider is null)
                return "contextProvider is not specified";

            return await contextProvider();
        }
    }
}
