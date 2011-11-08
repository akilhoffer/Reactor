using MassTransit;
using Reactor.Customization;
using Reactor.ServiceGrid;

namespace Reactor.InputService.Voice
{
    public class Service : ReactorServiceBase
    {
        public Service(IServiceBus serviceBus, ICustomizationManager customizationManager) : base(serviceBus, customizationManager)
        {
            Started += ((s, e) =>
                {

                });
        }
    }
}
