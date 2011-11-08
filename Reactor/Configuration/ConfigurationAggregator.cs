using System.Collections.Generic;
using System.Linq;

namespace Reactor.Configuration
{
    /// <summary>
    /// Provides members necessary for aggregation of multiple <seealso cref="IConfigurationStore"/> instances 
    /// and methods to obtain values from each.
    /// </summary>
    public interface IConfigurationAggregator
    {
        /// <summary>
        /// Gets the configuration stores registered with this instance.
        /// </summary>
        /// <value>The stores.</value>
        IEnumerable<IConfigurationStore> Stores { get; }

        /// <summary>
        /// Registers the configuration store with this instance. 
        /// </summary>
        /// <seealso cref="IConfigurationStore"/> instances are evaluated in the order they are registered. The 
        /// last store registered can override configuration values from other stores.
        /// <param name="configurationStore">The configuration store.</param>
        void RegisterConfigurationStore(IConfigurationStore configurationStore);

        /// <summary>
        /// Gets the configuration value located at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string GetConfigurationValue(string key);
    }

    public class ConfigurationAggregator : IConfigurationAggregator
    {
        #region Fields

        private readonly Queue<IConfigurationStore> _registrationQueue = new Queue<IConfigurationStore>();

        #endregion

        #region Implementation of IConfigurationAggregator

        /// <summary>
        /// Gets the configuration stores registered with this instance.
        /// </summary>
        /// <value>The stores.</value>
        public IEnumerable<IConfigurationStore> Stores
        {
            get
            {
                lock (this)
                {
                    return _registrationQueue.ToArray();
                }
            }
        }

        /// <summary>
        /// Registers the configuration store with this instance. 
        /// </summary>
        /// <seealso cref="IConfigurationStore"/> instances are evaluated in the order they are registered. The 
        /// last store registered can override configuration values from other stores.
        /// <param name="configurationStore">The configuration store.</param>
        public void RegisterConfigurationStore(IConfigurationStore configurationStore)
        {
            lock (this)
            {
                _registrationQueue.Enqueue(configurationStore);
            }
        }

        /// <summary>
        /// Gets the configuration value located at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetConfigurationValue(string key)
        {
            var value = string.Empty;
            lock (this)
            {
                var temporaryQueue = new Queue<IConfigurationStore>(_registrationQueue.Count);

                while (_registrationQueue.Any())
                {
                    var store = _registrationQueue.Dequeue();
                    temporaryQueue.Enqueue(store);

                    var tmpValue = store.GetConfigurationValue(key);
                    value = (string.IsNullOrEmpty(tmpValue)) ? value : tmpValue;
                }

                foreach (var configurationStore in temporaryQueue)
                    _registrationQueue.Enqueue(configurationStore);
            }

            return value;
        }

        #endregion
    }
}
