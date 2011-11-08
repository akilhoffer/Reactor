using System;
using System.Threading;
using System.Xml.Serialization;
using Reactor.ServiceGrid;
using Samurai.Wakizashi;

namespace Reactor.Entities
{
    public interface ISpark
    {
        void Start();
        void Stop();
        bool ShouldFireReaction();
        void React();
        string Name { get; set; }
        bool IsActive { get; set; }
        TimeSpan FireConditionCheckInterval { get; set; }
        IReaction Reaction { get; set; }
        ServiceIdentifier ReactionIdentifier { get; set; }
        RunState State { get; }
        DateTime FireConditionCheckAbsoluteStartTime { get; set; }
        event EventHandler<StateChangedEventArgs> StateChanged;
    }

    [Serializable]
    public abstract class Spark : DisposableBase, ISpark
    {
        private Timer _fireConditionCheckTimer;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Spark"/> class.
        /// <remarks>This constructor overload initializes the new instance the default value for FireConditionInterval.</remarks>
        /// </summary>
        protected Spark() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spark"/> class.
        /// </summary>
        /// <param name="fireConditionCheckInterval">The fire condition check interval. If a null value is provided, a default value of 10 seconds is assumed.</param>
        protected Spark(TimeSpan? fireConditionCheckInterval)
        {
            FireConditionCheckInterval = (fireConditionCheckInterval.HasValue)
                                             ? fireConditionCheckInterval.Value
                                             : TimeSpan.FromSeconds(10);
            State = RunState.Stopped;
        }

        #endregion

        #region Methods

        public void Start()
        {
            if (State == RunState.Started || State == RunState.Starting) return;

            TransitionState(RunState.Starting);

            _fireConditionCheckTimer = new Timer(FireConditionCheck, null, CalculateStartDelay(), FireConditionCheckInterval);

            TransitionState(RunState.Started);
        }

        public void Stop()
        {
            TransitionState(RunState.Stopping);

            Dispose(true);

            TransitionState(RunState.Stopped);
        }

        public abstract bool ShouldFireReaction();

        public abstract void React();

        private void FireConditionCheck(object state)
        {
            try
            {
                if (!ShouldFireReaction())
                    return;

                React();
            }
            catch (Exception)
            {
                TransitionState(RunState.Fault);
                Stop();

                throw;
            }
        }

        private void TransitionState(RunState newState)
        {
            if (State == newState || StateChanged == null) return;

            var previousState = State;
            State = newState;
            StateChanged(this, new StateChangedEventArgs
                                   {
                                       PreviousState = previousState,
                                       CurrentState = State
                                   });
        }

        private TimeSpan CalculateStartDelay()
        {
            // If no absolute start time was specified or start time is in the past, delay will no none
            if (FireConditionCheckAbsoluteStartTime == DateTime.MinValue || FireConditionCheckAbsoluteStartTime < DateTime.Now)
                return TimeSpan.FromMilliseconds(0);

            // Absolute start time must be in the future. Calculate time difference between now and then
            return (FireConditionCheckAbsoluteStartTime - DateTime.Now);
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public TimeSpan FireConditionCheckInterval { get; set; }

        public DateTime FireConditionCheckAbsoluteStartTime { get; set; }

        [XmlIgnore]
        public IReaction Reaction { get; set; }

        public ServiceIdentifier ReactionIdentifier { get; set; }

        [XmlIgnore]
        public RunState State { get; private set; }

        #endregion

        public event EventHandler<StateChangedEventArgs> StateChanged; 

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_fireConditionCheckTimer != null)
                _fireConditionCheckTimer.Dispose();
        }
    }

    public class StateChangedEventArgs : EventArgs
    {
        public RunState PreviousState { get; set; }
        public RunState CurrentState { get; set; }
    }
}
