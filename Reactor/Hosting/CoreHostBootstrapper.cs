using System.Diagnostics;
using System.Linq;
using Reactor.Exceptions;
using Reactor.ServiceGrid;

namespace Reactor.Hosting
{
    public static class CoreHostBootstrapper
    {
        public static void Bootstrap(string[] commandLineArguments)
        {
            // Ensure no other Reactor Core processes are running on this machine
            EnsureNoOtherCoreProcesses();

            var coreServiceHost = new CoreServiceHost();
            coreServiceHost.Host<ReactorCore>(commandLineArguments);
        }

        private static void EnsureNoOtherCoreProcesses()
        {
            if (Process.GetProcesses().Where(p => p.ProcessName.StartsWith("Reactor.CoreHost") && !p.ProcessName.StartsWith("Reactor.CoreHost.vshost")).Any())
                throw new FatalException("Only one Reactor Core process can be running on a single machine at a time.");
        }
    }
}
