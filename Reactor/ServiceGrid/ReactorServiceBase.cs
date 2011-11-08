using System;
using Reactor.Configuration;
using Reactor.Exceptions;
using log4net;

namespace Reactor.ServiceGrid
{
    public abstract class ReactorServiceBase : IReactorService
    {
        #region Fields

        protected static ILog Log;
        protected readonly IConfigurationAggregator ConfigurationAggregator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactorServiceBase"/> class.
        /// </summary>
        /// <param name="configurationAggregator">The configuration aggregator.</param>
        protected ReactorServiceBase(IConfigurationAggregator configurationAggregator)
        {
            if (configurationAggregator == null) throw new ArgumentNullException("configurationAggregator");

            ConfigurationAggregator = configurationAggregator;
            Log = LogManager.GetLogger(GetType());
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when this instance has been started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when this instance is starting.
        /// </summary>
        public event EventHandler Starting;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public ServiceIdentifier Identifier { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public virtual void Start()
        {
            Log.Info("Starting Reactor Service");

            try
            {
                if (Starting != null) Starting(this, EventArgs.Empty);

                OnStarting();

                OnBaseStarted();

                if (Started != null)
                    Started(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                throw new FatalException("Unable to start service.", ex);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public virtual void Stop()
        {
            OnShuttingDown();
        }

        #endregion

        /// <summary>
        /// Called when the instance is shutting down. This signals for concrete implementations to perform 
        /// their own teardown.
        /// </summary>
        protected virtual void OnShuttingDown() { }

        /// <summary>
        /// Called by the base class when the instance has been started. Derived classes can use 
        /// this to begin their work.
        /// </summary>
        protected virtual void OnBaseStarted() {}

        /// <summary>
        /// Called when the service instance is starting.
        /// </summary>
        protected virtual void OnStarting() {}
    }
}
