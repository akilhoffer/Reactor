using System;

namespace Reactor.ScheduleStream.Sparks
{
    /// <summary>
    /// Hourly schedule Spark that fires the configured Reaction every hour, on the hour.
    /// </summary>
    public class HourlySpark : ScheduleSpark
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HourlySpark"/> class.
        /// </summary>
        public HourlySpark() : base(GetStartTime()) {}

        private static TimeSpan GetStartTime()
        {
            var timeOfDay = DateTime.Now.TimeOfDay;
            var nextFullHour = TimeSpan.FromHours(Math.Ceiling(timeOfDay.TotalHours));
            return (nextFullHour - timeOfDay);
        }
    }
}
