using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Reactor.Composition;
using Reactor.Configuration;
using Reactor.Customization;
using Reactor.Exceptions;
using Reactor.FileSystem;
using Reactor.Resources;
using Reactor.ServiceGrid;
using log4net;
using log4net.Config;

namespace Reactor.Hosting
{
    internal abstract class ServiceHostFoundation
    {
        #region Fields

        protected static readonly ILog Log = LogManager.GetLogger(typeof(ServiceHostFoundation));
        protected IReactorService ReactorService;

        #endregion

        #region Constructors

        static ServiceHostFoundation()
        {
            if (IsWindowsService())
            {
                var appDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Trace.WriteLine(string.Format("Setting current directory to current AppDomain base directory: {0}", appDomainBaseDirectory));
                Directory.SetCurrentDirectory(appDomainBaseDirectory);
            }

            XmlConfigurator.Configure(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config")));
        }

        #endregion

        #region Properties

        public abstract string ServiceName { get; }

        public abstract string ServiceDescription { get; }

        #endregion

        #region Protected Methods

        protected static string DetermineDefaultServiceName()
        {
            return Process.GetCurrentProcess().MainModule.ModuleName.Replace(".exe", String.Empty).Replace(".vshost", String.Empty);
        }

        protected virtual void Install()
        {
            var servicePath = Assembly.GetEntryAssembly().Location;

            if (Log.IsDebugEnabled) Log.DebugFormat("Attempting to install Windows Service: {0}", ServiceName);

            if (ServiceInstaller.InstallService(servicePath, ServiceName, ServiceName, ServiceDescription, true, false))
                Log.Debug("Windows Service installed.");
            else
                Log.Error("Failed to install Windows Service.");
        }

        protected virtual void Uninstall()
        {
            if (Log.IsDebugEnabled) Log.DebugFormat("Attempting to uninstall Windows Service: {0}", ServiceName);

            if (!ServiceInstaller.UnInstallService(ServiceName))
                Log.ErrorFormat("Failure reported by ServiceInstaller while attempting to uninstall service: {0}", ServiceName);
        }

        #endregion

        #region Public Methods

        public virtual void Host<T>(string[] commandLineArguments) where T : IReactorService
        {
            try
            {
                if (Log.IsDebugEnabled) Log.DebugFormat("Command line: {0}", Environment.CommandLine);

                ProcessCommandLine<T>(commandLineArguments);
            }
            catch (FatalException fatalException)
            {
                const string msg = "Host cannot continue due to a fatal exception.";
                if (Log.IsFatalEnabled)
                    Log.Fatal(msg, fatalException);

                Console.WriteLine(msg);
                Console.WriteLine(fatalException);

                DisplayFatalExit();
            }
            catch (Exception ex)
            {
                const string msg = "An unhandled exception reached the service host.";
                if (Log.IsErrorEnabled)
                    Log.Error(msg, ex);

                Console.WriteLine(msg);
                Console.WriteLine(ex);

                DisplayFatalExit();
            }
        }

        #endregion

        #region Private Methods

        private void ProcessCommandLine<T>(string[] commandLineArguments) where T : IReactorService
        {
            if (commandLineArguments.Contains("-i"))
            {
                Log.Debug("Install switch detected.");

                Install();
            }
            else if (commandLineArguments.Contains("-u"))
            {
                Log.Debug("Uninstall switch detected.");

                Uninstall();
                return;
            }

            if (commandLineArguments.Length == 0 || commandLineArguments.Contains("-run"))
            {
                Log.Debug("Run switch detected or defaulted due to no command line switches.");

                var runAsConsole = IsConsoleMode(commandLineArguments);
                if (Log.IsDebugEnabled) Log.DebugFormat("Console mode: {0}", (runAsConsole) ? "on" : "off");

                Run<T>(runAsConsole);
            }
        }

        private void Run<T>(bool runAsConsole) where T : IReactorService
        {
            if (Log.IsDebugEnabled) Log.DebugFormat("Run called. RunAsConsole flag: {0}", runAsConsole);

            // Setup configuration aggregator
            var configurationAggregator = new ConfigurationAggregator();
            configurationAggregator.RegisterConfigurationStore(new AppConfigStore());

            // Store reference to configuration aggregator so the initialization customizers can add to it if needed
            InitializationContext.ConfigurationAggregator = configurationAggregator;

            // Store service name so the initialization customizers can use it if needed
            InitializationContext.ServiceName = ServiceName;

            // Call upon customization manager to seek out initialization plug ins
            var customizationManager = new InitializationCustomizationManager(new DirectoryBasedComposer(), new PhysicalFileSystem());
            customizationManager.DiscoverCustomizers(AppDomain.CurrentDomain.BaseDirectory);

            if (customizationManager.Customizers.Any())
                customizationManager.RunAll(); // Run all discovered customizers

            ReactorService = (IReactorService) Activator.CreateInstance(typeof (T), InitializationContext.Bus, InitializationContext.ConfigurationAggregator);
            ReactorService.Identifier = new ServiceIdentifier(InitializationContext.ServiceName, "1.0.0.0");    //TODO: When versioning is supported, stop hardcoding this!

            // Set global reference
            ServiceContext.ServiceInstance = ReactorService;

            if (runAsConsole)
            {
                Log.Debug("Starting ReactorService directly because of console flag.");

                ReactorService.Start();

                Console.WriteLine(CommonResources.HostService_Run_Type__exit__to_quit);

                while (Console.ReadLine() != "exit")
                {
                }

                ReactorService.Stop();
            }
            else
            {
                if (Log.IsDebugEnabled)
                    Log.DebugFormat("Handing off execution to the Windows Service. Service name: {0}", ServiceName);

                var windowsServiceWrapper = new ServiceWrapper(ReactorService);
                ServiceBase.Run(windowsServiceWrapper);

                if (Log.IsDebugEnabled) Log.Debug("ServiceBase.Run called.");
            }
        }

        private static bool IsConsoleMode(string[] commandLineArguments)
        {
            if (Debugger.IsAttached) return true;
            if (commandLineArguments.Contains("-console") || commandLineArguments.Contains("-c")) return true;
            if (Environment.UserInteractive) return true;

            return false;
        }

        private static void DisplayFatalExit()
        {
            Console.WriteLine();
            Console.WriteLine(CommonResources.FatalExit_Press_any_key_to_quit);
            Console.ReadKey();
        }

        private static bool IsWindowsService()
        {
            // Currently, we're determining this by seeing if the working directory is the system32 directory.
            // If a better way is discovered, this needs to change

            var sys32DirectoryPath = Environment.ExpandEnvironmentVariables("%windir%\\system32");
            var currentDirectory = Directory.GetCurrentDirectory();

            if (currentDirectory == sys32DirectoryPath)
            {
                var msg = "Running as Windows Service";
                Trace.WriteLine(msg);    
                Log.Debug(msg);

                return true;
            }

            return false;
        }

        #endregion
    }
}
