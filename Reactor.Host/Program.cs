using Reactor.Hosting;

namespace Reactor.CoreHost
{
    class Program
    {
        static void Main(string[] args)
        {
            CoreHostBootstrapper.Bootstrap(args);
        }
    }
}
