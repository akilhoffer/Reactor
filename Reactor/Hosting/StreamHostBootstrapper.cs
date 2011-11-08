using Reactor.ServiceGrid;

namespace Reactor.Hosting
{
    public static class StreamHostBootstrapper
    {
        public static void Bootstrap<T>(string[] commandLineArguments) where T : IReactorService
        {
            var streamServiceHost = new StreamServiceHost();
            streamServiceHost.Host<T>(commandLineArguments);
        }
    }
}
