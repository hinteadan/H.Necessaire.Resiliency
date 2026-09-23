using System.Collections.Generic;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Abstractions
{
    /// <summary>
    /// Whatever class implements implements this should:
    /// 
    /// Either start a daemon that periodically measures and updates the resiliency score for the inner registred services
    /// ~ OR ~
    /// Run the measurement on-demand when calling any of its methods and obviously cache the result for some time before measuring again
    /// 
    /// BUT
    /// 
    /// if the measurement is quite heavy like HTTP call duration for 3 or more services... the daemon is preferred
    /// 
    /// We start with a default order by knowing the most probable service to be the most resilient and then the daemon updates the order as the app runs
    /// 
    /// A base (abstract) class with the daemon approach is to be implemented
    /// 
    /// </summary>
    /// <typeparam name="TService">Underlying services type</typeparam>
    public interface ImAnHResilientServiceProvider<TService>
    {
        Task<OperationResult<TService>> GetBestKnownServiceInstance();
        Task<OperationResult<KeyValuePair<string, TService>>> GetBestKnownServiceInstanceWithID();
        Task<OperationResult<TService[]>> GetAllServiceInstancesOrderedByResiliency();
        Task<OperationResult<KeyValuePair<string, TService>[]>> GetAllServiceInstancesOrderedByResiliencyWithIDs();
        Task<OperationResult<ImAnHServiceResiliencyRunner<TService>>> GetResiliencyRunner();
    }
}
