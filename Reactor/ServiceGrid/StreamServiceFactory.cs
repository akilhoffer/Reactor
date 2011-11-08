using System;
using System.IO;
using Reactor.Configuration;
using Reactor.FileSystem;
using Reactor.Resources;
using Reactor.ServiceGrid.Workflows;
using log4net;
using Directory = Magnum.FileSystem.Directory;

namespace Reactor.ServiceGrid
{
    internal class StreamServiceFactory
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(typeof (StreamServiceFactory));
        private const string DefaultStreamServicePackageDirectoryPath = "C:\\Reactor\\StreamServicePackages";
        private const string DefaultStreamServicesInstallRootPath = "C:\\Reactor\\StreamServices";
        private readonly IConfigurationAggregator _configurationAggregator;
        private PackageDirectoryWatcher _packageDirectoryWatcher;
        private IDisposable _cleanup;
        private string _streamServicesInstallRootPath;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamServiceFactory"/> class.
        /// </summary>
        /// <param name="configurationAggregator">The configuration aggregator.</param>
        public StreamServiceFactory(IConfigurationAggregator configurationAggregator)
        {
            if (configurationAggregator == null) throw new ArgumentNullException("configurationAggregator");
            _configurationAggregator = configurationAggregator;
        }

        public void Start()
        {
            // Find out where Stream packages are stored
            var packageDirectoryPath = _configurationAggregator.GetConfigurationValue(CommonResources.Config_StreamPackageDirectoryPath);
            if (string.IsNullOrEmpty(packageDirectoryPath))
            {
                Log.WarnFormat("No Stream Service package directory found in AppSettings at key '{0}'. Defaulting to: {1}", "StreamPackageDirectoryPath", DefaultStreamServicePackageDirectoryPath);
                packageDirectoryPath = DefaultStreamServicePackageDirectoryPath;
            }

            // Find out where Stream Services get installed
            _streamServicesInstallRootPath = _configurationAggregator.GetConfigurationValue(CommonResources.Config_StreamServiceInstallationDirectoryPath);
            if (string.IsNullOrEmpty(_streamServicesInstallRootPath))
            {
                Log.WarnFormat("No Stream Services install root directory found in AppSettings at key '{0}'. Defaulting to: {1}", "StreamServiceInstallationDirectoryPath", DefaultStreamServicesInstallRootPath);
                _streamServicesInstallRootPath = DefaultStreamServicesInstallRootPath;
            }

            // Create configured Stream Services package directory if it doesnt already exist
            if (!System.IO.Directory.Exists(packageDirectoryPath))
                System.IO.Directory.CreateDirectory(packageDirectoryPath);

            // Create configured Stream Services directory if it doesnt already exist
            if (!System.IO.Directory.Exists(_streamServicesInstallRootPath))
                System.IO.Directory.CreateDirectory(_streamServicesInstallRootPath);

            _packageDirectoryWatcher = new PackageDirectoryWatcher();
            _cleanup = _packageDirectoryWatcher.Watch(packageDirectoryPath, ProcessStreamPackage);
        }

        public void Stop()
        {
            if(_cleanup != null)
                _cleanup.Dispose();
        }

        private void ProcessStreamPackage(Directory directory)
        {
            var serviceName = directory.Name.Name.GetName().Replace(".zip", string.Empty);
            var workflowContext = new InstallServiceWorkflowContext
                                      {
                                          FileSystem = new PhysicalFileSystem(),
                                          ServiceName = serviceName,
                                          Controller = Hosting.ServiceInstaller.GetServiceByName(serviceName),
                                          ServiceInstallsRootPath = _streamServicesInstallRootPath,
                                          ServiceInstallDirectoryPath = Path.Combine(_streamServicesInstallRootPath, serviceName),
                                          ServiceZipFile = directory
                                      };
            var workflow = new InstallServiceWorkflow(workflowContext);
            workflow.OnFailed(args => Log.ErrorFormat("Failed to install Stream Service. Reason: {0}", args.Exception.Message));
            workflow.Start();
        }
    }
}
