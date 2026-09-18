namespace H.Necessaire.Resiliency
{
    public static class DI
    {
        public static T WithHNecessaireResiliency<T>(this T deps) where T : ImADependencyRegistry
        {
            return deps;
        }
    }
}
