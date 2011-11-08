using System;

namespace Reactor.ScheduleStream.Sparks
{
    public class DailySpark : ScheduleSpark
    {
        public DailySpark() : base(TimeSpan.FromDays(1)) {}

        public void SetDailyFireTime(DateTime time)
        {
            
        }
    }
}
