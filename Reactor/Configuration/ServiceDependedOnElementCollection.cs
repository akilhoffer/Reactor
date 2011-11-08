using System.Configuration;

namespace Reactor.Configuration
{
    [ConfigurationCollection(typeof(ServiceDependedOnElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "service")]
    public class ServiceDependedOnElementCollection : ConfigurationElementCollection<ServiceDependedOnElement>
    {
    }
}
