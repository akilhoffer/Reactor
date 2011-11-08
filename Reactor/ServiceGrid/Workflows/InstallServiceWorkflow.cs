using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Magnum.FileSystem;
using Reactor.Extensions;
using Reactor.FileSystem;
using Samurai.Wakizashi.Workflow;
using Samurai.Wakizashi.Workflow.Fluent;
using Directory = Magnum.FileSystem.Directory;

namespace Reactor.ServiceGrid.Workflows
{
    internal class InstallServiceWorkflow : SequentialWorkflow<InstallServiceWorkflowContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallServiceWorkflow"/> class.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        public InstallServiceWorkflow(InstallServiceWorkflowContext ctx)
        {
            Context = ctx;

            Configure(RegisterWorkflowSteps);
        }

        private void RegisterWorkflowSteps(ISequentialWorkflow<InstallServiceWorkflowContext> w)
        {
            w.Do(AttemptExistingServiceStop)
                .OnThread(ThreadType.Task)
                .If(c => c.Controller != null).Done();

            // If the service install directory already exists, delete it
            w.Do(RecreateServiceInstallDirectory)
                .OnThread(ThreadType.Task).Done();

            // Create service install directory
            w.Do(() => Context.FileSystem.CreateDirectory(Context.ServiceInstallDirectoryPath))
                .OnThread(ThreadType.Task)
                .Named("CreateServiceInstallDirectory")
                .If(c => Context.FileSystem.DirectoryExists(Context.ServiceInstallDirectoryPath)).Done();

            // Copy service contents into install directory
            w.Do(() => Context.ServiceZipFile.CopyTo(DirectoryName.GetDirectoryName(Context.ServiceInstallDirectoryPath)))
                .OnThread(ThreadType.Task)
                .Named("CopyPackageContentsToServiceInstallDirectory")
                .If(c => Context.FileSystem.DirectoryExists(Context.ServiceInstallDirectoryPath)).Done();

            // Run main executable for the service, instructing it to install itself as a Windows Service
            w.Do(InstallStreamHostAsWindowsService).Done();

            // Start service
            w.Do(StartService).Done();
        }

        private void RecreateServiceInstallDirectory()
        {
            // Delete it if it already exists
            if (Context.FileSystem.DirectoryExists(Context.ServiceInstallDirectoryPath))
                Context.FileSystem.DeleteDirectory(Context.ServiceInstallDirectoryPath, true);

            // Create new one
            Context.FileSystem.CreateDirectory(Context.ServiceInstallDirectoryPath);
        }

        private void StartService()
        {
            string startFailureReason;
            if (!Context.Controller.TryStart(Context.ServiceStartTimeout, out startFailureReason))
                throw new ApplicationException(startFailureReason);
        }

        private void AttemptExistingServiceStop()
        {
            string stopFailureReason;
            Context.Controller.TryStop(TimeSpan.FromSeconds(30), out stopFailureReason);

            if (!string.IsNullOrEmpty(stopFailureReason))
                throw new ApplicationException(string.Format("Unable to stop {0} service in a reasonable amount of time. Reason: {1}", Context.ServiceName, stopFailureReason));
        }

        private void InstallStreamHostAsWindowsService()
        {
            var executablePath = Path.Combine(Context.ServiceInstallDirectoryPath, string.Format("{0}.exe", Context.ServiceName));

            var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "-i"
                };
            var process = Process.Start(startInfo);
            process.WaitForExit((int)Context.ServiceInstallTimeout.TotalMilliseconds);

            if (process.HasExited) return;

            // Process did not exit. Kill it and throw an exception
            process.Kill();
            throw new ApplicationException(
                "Service executable did not finish instruction to install as Windows Service within alloted timeout period.");
        }
    }

    internal class InstallServiceWorkflowContext : IWorkflowContext
    {
        public InstallServiceWorkflowContext()
        {
            ServiceInstallTimeout = TimeSpan.FromMinutes(1);
            ServiceStartTimeout = ServiceInstallTimeout;
        }

        public IFileSystem FileSystem { get; set; }
        public string ServiceName { get; set; }
        public ServiceController Controller { get; set; }
        public string ServiceInstallsRootPath { get; set; }
        public string ServiceInstallDirectoryPath { get; set; }
        public Directory ServiceZipFile { get; set; }
        public TimeSpan ServiceInstallTimeout { get; set; }
        public TimeSpan ServiceStartTimeout { get; set; }
    }
}
