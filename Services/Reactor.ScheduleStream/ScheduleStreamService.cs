using Reactor.Configuration;
using Reactor.ServiceGrid;

namespace Reactor.ScheduleStream
{
    internal class ScheduleStreamService : StreamService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactorServiceBase"/> class.
        /// </summary>
        public ScheduleStreamService(IConfigurationAggregator configurationAggregator) : base(configurationAggregator)
        {}

        protected override void OnBaseStarted()
        {
            Log.Info("Service started");
        }
    }
}
