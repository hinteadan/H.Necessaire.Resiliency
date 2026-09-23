using H.Necessaire.Resiliency.Abstractions.DataModels;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Abstractions
{
    public interface ImAnHResiliencyMeasurer
    {
        Task<OperationResult<ImAnHResiliencyMeasurement>> MeasureResiliency(ImAnHResiliencyMeasurementContext measurementContext);
    }
}
