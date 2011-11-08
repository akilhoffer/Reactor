using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactor.Entities;

namespace Reactor.ScheduleStream.Sparks
{
    public class WeeklySpark : ScheduleSpark
    {
        public WeeklySpark(TimeSpan interval) : base(interval)
        {
        }
    }
}
