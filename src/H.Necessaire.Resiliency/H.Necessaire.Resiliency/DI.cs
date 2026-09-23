using H.Necessaire.Resiliency.Abstractions;
using H.Necessaire.Resiliency.Abstractions.DataModels;
using H.Necessaire.Resiliency.Debugging;
using H.Necessaire.Resiliency.Measurers;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency
{
    public static class DI
    {
        public static T WithHNecessaireResiliency<T>(this T deps) where T : ImADependencyRegistry
        {
            deps
                .WithMeasurers()
                .WithDebugging()
                ;

            return deps;
        }

        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            Func<Task<OperationResult<ImAnHResiliencyMeasurementContext>>> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;

        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<Task<OperationResult<ImAnHResiliencyMeasurementContext>>> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;


        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            Func<Task<ImAnHResiliencyMeasurementContext>> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            Func<OperationResult<ImAnHResiliencyMeasurementContext>> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            Func<ImAnHResiliencyMeasurementContext> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;

        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            Task<OperationResult<ImAnHResiliencyMeasurementContext>> context
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, context)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            Task<ImAnHResiliencyMeasurementContext> context
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, context)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            OperationResult<ImAnHResiliencyMeasurementContext> context
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, context)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImADependencyProvider, TService> instanceFactory,
            ImAnHResiliencyMeasurementContext context
        )
            => new HServiceResiliencyProvider<TService>(id, instanceFactory, context)
            .And(x => x.ReferDependencies(deps))
            ;


        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<Task<ImAnHResiliencyMeasurementContext>> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<OperationResult<ImAnHResiliencyMeasurementContext>> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Func<ImAnHResiliencyMeasurementContext> contextProvider
        )
            => new HServiceResiliencyProvider<TService>(id, contextProvider)
            .And(x => x.ReferDependencies(deps))
            ;


        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Task<OperationResult<ImAnHResiliencyMeasurementContext>> context
        )
            => new HServiceResiliencyProvider<TService>(id, context)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            Task<ImAnHResiliencyMeasurementContext> context
        )
            => new HServiceResiliencyProvider<TService>(id, context)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            OperationResult<ImAnHResiliencyMeasurementContext> context
        )
            => new HServiceResiliencyProvider<TService>(id, context)
            .And(x => x.ReferDependencies(deps))
            ;
        public static ImAnHServiceResiliencyProvider<TService> NewHServiceResiliencyProvider<TService>(
            this ImADependencyProvider deps,
            string id,
            ImAnHResiliencyMeasurementContext context
        )
            => new HServiceResiliencyProvider<TService>(id, context)
            .And(x => x.ReferDependencies(deps))
            ;
    }
}
