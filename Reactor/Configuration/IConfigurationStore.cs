namespace Reactor.Configuration
{
    public interface IConfigurationStore
    {
        /// <summary>
        /// Stores the configuration value at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void StoreConfigurationValue(string key, string value);

        /// <summary>
        /// Gets the configuration value at the specified key.
        /// <remarks>This method first searches all cached configuration elements. If none match the key 
        /// specified, a search in application configuration will be performed.</remarks>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The string representation of the value found at the specified key.</returns>
        string GetConfigurationValue(string key);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();
    }
}
