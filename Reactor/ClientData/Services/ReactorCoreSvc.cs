using Reactor.ClientData.Models;

namespace Reactor.ClientData.Services
{
    public class ReactorCoreSvc : ICoreService
    {
        #region Implementation of ICoreService

        public ReactorCore[] GetCores()
        {
            // For now, we only support this hosting Core. In the future, provide support 
            //  for Cores to discover each other and provide details on these discovered 
            //  Cores via this method.

            var dto = new ReactorCore
                          {
                              Name = ServiceContext.ServiceInstance.Identifier.Name,
                          };

            return new[] {dto};
        }

        #endregion
    }
}
