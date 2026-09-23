namespace H.Necessaire.Resiliency.Measurers
{
    internal static class DI
    {
        public static T WithMeasurers<T>(this T deps) where T : ImADependencyRegistry
        {
            deps
                .Register<HNetworkPingMeasurer>(() => new HNetworkPingMeasurer())
                .Register<HExecutionTimeMeasurer>(() => new HExecutionTimeMeasurer())
                ;
            return deps;
        }
    }
}
