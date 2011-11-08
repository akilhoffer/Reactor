using System;
using Reactor.Entities;

namespace Reactor.ScheduleStream.Sparks
{
    public abstract class ScheduleSpark : Spark
    {
        protected ScheduleSpark(TimeSpan interval) : base(interval) {}

        #region Overrides of Spark

        public override bool ShouldFireReaction()
        {
            // Since the interval specified in the constructor is all we're really concerned with 
            //  in this particular type of Spark, this can simply return true.
            return true;
        }

        public override void React()
        {
            Reaction.Execute();
        }

        #endregion
    }
}
