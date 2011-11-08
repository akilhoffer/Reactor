using MassTransit;

namespace Reactor
{
    public static class ServiceBusExtensions
    {
        /// <summary>
        /// Shutdowns the service bus by disposing the instance.
        /// </summary>
        /// <param name="serviceBus">The service bus.</param>
        public static void Shutdown(this IServiceBus serviceBus)
        {
            if(serviceBus != null)
                serviceBus.Dispose();
        }
    }
}
