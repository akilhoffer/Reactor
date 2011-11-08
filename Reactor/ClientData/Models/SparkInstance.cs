using System.Runtime.Serialization;

namespace Reactor.ClientData.Models
{
    [DataContract]
    public class SparkInstance
    {
        [DataMember]
        public Reaction Reaction { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
