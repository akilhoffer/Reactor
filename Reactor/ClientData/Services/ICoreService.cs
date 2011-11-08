using System.ServiceModel;
using Reactor.ClientData.Models;

namespace Reactor.ClientData.Services
{
    [ServiceContract]
    public interface ICoreService
    {
        /// <summary>
        /// Provides data representations of discovered Reactor Cores, including the Core hosting 
        /// this service.
        /// </summary>
        /// <returns>Representations of Reactor Cores.</returns>
        [OperationContract]
        ReactorCore[] GetCores();
    }
}
