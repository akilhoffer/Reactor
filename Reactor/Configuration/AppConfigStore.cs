using System;
using System.Configuration;
using Reactor.Resources;

namespace Reactor.Configuration
{
    public class AppConfigStore : IConfigurationStore
    {
        #region Implementation of IConfigurationStore

        /// <summary>
        /// Stores the configuration value at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void StoreConfigurationValue(string key, string value)
        {
            throw new NotSupportedException(CommonResources.Error_UpdatesToAppConfigNotYetSupportedByThisInstance);
        }

        /// <summary>
        /// Gets the configuration value at the specified key.
        /// <remarks>This method first searches all cached configuration elements. If none match the key 
        /// specified, a search in application configuration will be performed.</remarks>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The string representation of the value found at the specified key.</returns>
        public string GetConfigurationValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException(CommonResources.Error_UpdatesToAppConfigNotYetSupportedByThisInstance);
        }

        #endregion
    }
}