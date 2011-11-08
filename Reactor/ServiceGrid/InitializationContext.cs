using MassTransit;
using Reactor.Configuration;

namespace Reactor.ServiceGrid
{
    public static class InitializationContext
    {
        public static IServiceBus Bus { get; set; }
        public static IConfigurationAggregator ConfigurationAggregator { get; set; }
        public static string ServiceName { get; set; }
    }
}
