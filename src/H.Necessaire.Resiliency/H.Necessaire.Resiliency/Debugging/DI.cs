using H.Necessaire.Resiliency.Abstractions;
using System.Diagnostics;

namespace H.Necessaire.Resiliency.Debugging
{
    internal static class DI
    {
#if DEBUG
        const bool isDebug = true;
#else
        const bool isDebug = false;
#endif
        public static T WithDebugging<T>(this T deps) where T : ImADependencyRegistry
        {
            if (!isDebug || !Debugger.IsAttached)
                return deps;



            deps
                .RegisterAlwaysNew<ResiliencyTestService>(() => new ResiliencyTestService())
                .Register<TestServiceMeasurer>(() => new TestServiceMeasurer())
                .Register<ImAnHResilientServiceProvider<ResiliencyTestService>>(() => new ResiliencyTestServiceProvider())
                .Register<HNecessaireResiliencyDebugger>(() => new HNecessaireResiliencyDebugger())
                ;

            return deps;
        }
    }
}
