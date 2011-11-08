using System.Collections.Generic;
using Reactor.Configuration;

namespace Reactor.Hosting.Internal
{
    /// <summary>
    /// Information about a candidate Windows Service. This class was copied from the Daemoniq project 
    /// and modified. http://code.google.com/p/daemoniq
    /// </summary>
    internal class CandidateWindowsServiceInfo
    {
        private readonly List<string> _servicesDependedOn;

        public CandidateWindowsServiceInfo()
        {
            StartMode = StartMode.Manual;
            _servicesDependedOn = new List<string>();
            RecoveryOptions = new ServiceRecoveryOptions();
        }

        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public StartMode StartMode { get; set; }
        public ServiceRecoveryOptions RecoveryOptions { get; set; }
        public List<string> ServicesDependedOn
        {
            get { return _servicesDependedOn; }
        }

        public static CandidateWindowsServiceInfo FromConfiguration(ServiceElement serviceElement)
        {
            return new CandidateWindowsServiceInfo
            {
                ServiceName = serviceElement.ServiceName,
                DisplayName = serviceElement.DisplayName,
                Description = serviceElement.Description,
                StartMode = serviceElement.StartMode,
                RecoveryOptions =
                    ServiceRecoveryOptions.FromConfiguration(serviceElement.RecoveryOptions)
            };
        }
    }
}
