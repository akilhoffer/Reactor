using System.Configuration;

namespace Reactor.Configuration
{
    public class ServiceDependedOnElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)(base["name"]); }
            set { base["name"] = value; }
        }
    }
}
