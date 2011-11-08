using Reactor.Hosting;

namespace Reactor.ScheduleStream
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamHostBootstrapper.Bootstrap<ScheduleStreamService>(args);
        }
    }
}
