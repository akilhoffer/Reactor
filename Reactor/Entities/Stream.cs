using System.Collections.Generic;

namespace Reactor.Entities
{
    public class Stream
    {
        public string Name { get; set; }

        public IList<ISpark> Sparks { get; set; }
    }
}
