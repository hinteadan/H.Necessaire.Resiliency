namespace H.Necessaire.Resiliency.Abstractions
{
    public interface ImAnHServiceResiliencyProvider<TService> : ImAnHResiliencyMeasurementContextProvider
    {
        TService GetServiceInstance();
    }
}
