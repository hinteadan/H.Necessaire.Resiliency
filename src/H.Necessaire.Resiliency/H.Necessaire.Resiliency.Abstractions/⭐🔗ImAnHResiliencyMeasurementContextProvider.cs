using H.Necessaire.Resiliency.Abstractions.DataModels;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Abstractions
{
    public interface ImAnHResiliencyMeasurementContextProvider : IStringIdentity
    {
        Task<OperationResult<ImAnHResiliencyMeasurementContext>> GetLatestResiliencyMeasurementContext();
    }
}
