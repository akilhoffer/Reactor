using Reactor.ServiceGrid;

namespace Reactor
{
    internal static class ServiceContext
    {
        #region Fields

        private static readonly object ServiceInstanceSync = new object();
        private static IReactorService _serviceInstance;

        #endregion

        /// <summary>
        /// Gets or sets the IReactorService instance associated with this process.
        /// </summary>
        /// <value>
        /// The IReactorService instance.
        /// </value>
        public static IReactorService ServiceInstance
        {
            get
            {
                IReactorService instance;
                lock (ServiceInstanceSync)
                {
                    instance = _serviceInstance;
                }
                return instance;
            }

            set
            {
                lock (ServiceInstanceSync)
                {
                    _serviceInstance = value;
                }
            }
        }
    }
}
