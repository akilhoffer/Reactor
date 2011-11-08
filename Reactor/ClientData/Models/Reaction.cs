using System;
using System.Runtime.Serialization;

namespace Reactor.ClientData.Models
{
    [DataContract]
    public class Reaction
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TypeName { get; set; }
    }
}
