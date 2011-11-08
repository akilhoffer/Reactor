using System.Configuration;
using Reactor.Hosting;

namespace Reactor.Configuration
{
    /// <summary>
    /// Represents a configuration element that allows configuration of a service. This 
    /// class was copied from the Daemoniq project and modified. http://code.google.com/p/daemoniq
    /// </summary>
    public class ServiceElement : ConfigurationElement
    {
        public ServiceElement()
        {
            StartMode = StartMode.Manual;
        }

        [ConfigurationProperty("serviceName", IsRequired = true, IsKey = true)]
        public string ServiceName
        {
            get { return (string)(base["serviceName"]); }
            set { base["serviceName"] = value; }
        }

        [ConfigurationProperty("displayName")]
        public string DisplayName
        {
            get { return (string)(base["displayName"]); }
            set { base["displayName"] = value; }
        }

        [ConfigurationProperty("description")]
        public string Description
        {
            get { return (string)(base["description"]); }
            set { base["description"] = value; }
        }

        [ConfigurationProperty("serviceStartMode")]
        public StartMode StartMode
        {
            get { return (StartMode)(base["serviceStartMode"]); }
            set { base["serviceStartMode"] = value; }
        }

        [ConfigurationProperty("recoveryOptions")]
        public RecoveryOptionsElement RecoveryOptions
        {
            get { return (RecoveryOptionsElement)this["recoveryOptions"]; }
            set { this["recoveryOptions"] = value; }
        }

        [ConfigurationProperty("servicesDependedOn",
           IsDefaultCollection = true, IsRequired = false)]
        public ServiceDependedOnElementCollection ServicesDependedOn
        {
            get { return (ServiceDependedOnElementCollection)this["servicesDependedOn"]; }
        }
    }
}
