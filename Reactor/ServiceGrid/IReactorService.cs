using System;

namespace Reactor.ServiceGrid
{
    public interface IReactorService
    {
        #region Events

        /// <summary>
        /// Occurs when this instance has been started.
        /// </summary>
        event EventHandler Started;

        /// <summary>
        /// Occurs when this instance is starting.
        /// </summary>
        event EventHandler Starting;

        #endregion

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        ServiceIdentifier Identifier { get; set; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }

    public class ReactorServiceStartingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether to cancel the starting of the service.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        public bool Cancel { get; set; }
    }
}