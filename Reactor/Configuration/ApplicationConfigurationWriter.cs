using System;
using System.IO;
using System.Xml;
using log4net;

namespace Reactor.Configuration
{
    /// <summary>
    /// This writer is intended to write to a particular application configuration file. It 
    /// is not intended to write to the currently running application's configuration file.
    /// </summary>
    public class ApplicationConfigurationWriter
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationConfigurationWriter));
        private const string AppSettingsSectionName = "appSettings";
        private const string ConnectionStringsSectionName = "connectionStrings";
        private const string ConnectionStringElementName = "connectionString";
        private readonly string _configurationFilePath;
        private XmlDocument _xmlDocument;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationConfigurationWriter"/> class.
        /// </summary>
        /// <param name="configurationFilePath">The configuration file path.</param>
        public ApplicationConfigurationWriter(string configurationFilePath)
        {
            if (string.IsNullOrEmpty(configurationFilePath)) throw new ArgumentNullException("configurationFilePath");
            if (!File.Exists(configurationFilePath)) throw new FileNotFoundException("Cannot find configuration file.", configurationFilePath);

            _configurationFilePath = configurationFilePath;
        }

        /// <summary>
        /// Writes an appSetting to the configuration file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void WriteSetting(string key, string value)
        {
            LoadConfigDocument();

            // Retrieve appSettings node
            var appSettingsNode = _xmlDocument.SelectSingleNode("//" + AppSettingsSectionName) ?? CreateSection(AppSettingsSectionName);

            // Select the 'add' element that contains the key
            var elem = (XmlElement)appSettingsNode.SelectSingleNode(string.Format("//add[@key='{0}']", key));

            if (elem != null)
            {
                // Add value for key
                elem.SetAttribute("value", value);
            }
            else
            {
                // Key was not found so create the 'add' element and set it's key/value attributes 
                elem = _xmlDocument.CreateElement("add");
                elem.SetAttribute("key", key);
                elem.SetAttribute("value", value);
                appSettingsNode.AppendChild(elem);
            }
            _xmlDocument.Save(_configurationFilePath);
        }

        /// <summary>
        /// Removes an appSetting setting from the configuration file.
        /// </summary>
        /// <param name="key">The key.</param>
        public void RemoveSetting(string key)
        {
            LoadConfigDocument();

            // Retrieve appSettings node
            var node = _xmlDocument.SelectSingleNode("//appSettings");
            if (node == null) return;

            // Remove 'add' element with coresponding key
            node.RemoveChild(node.SelectSingleNode(string.Format("//add[@key='{0}']", key)));

            // Save changes
            _xmlDocument.Save(_configurationFilePath);
        }

        /// <summary>
        /// Writes the specified connection string to the application configuration file.
        /// </summary>
        public void WriteConnectionString(string name, string value)
        {
            LoadConfigDocument();

            // Retrieve connectionString node
            var connectionStringsNode = _xmlDocument.SelectSingleNode("//" + ConnectionStringsSectionName) ?? CreateSection(ConnectionStringsSectionName);

            // Select the 'add' element that contains the connection string by name
            var elem = (XmlElement)connectionStringsNode.SelectSingleNode(string.Format("//add[@name='{0}']", name));

            if (elem != null)
            {
                // Add value for name
                elem.SetAttribute(ConnectionStringElementName, value);
            }
            else
            {
                // Connection string was not found so create the 'add' element and set it's name/connectionString attributes 
                Log.DebugFormat("Connection string '{0}' not found. Creating one...", name);

                elem = _xmlDocument.CreateElement("add");
                elem.SetAttribute("name", name);
                elem.SetAttribute(ConnectionStringElementName, value);
                connectionStringsNode.AppendChild(elem);

                Log.DebugFormat("Created connection string '{0}' with value: {1}", name, value);
            }

            _xmlDocument.Save(_configurationFilePath);
        }

        /// <summary>
        /// Removes the specified connection string from the application configuration file.
        /// </summary>
        public void RemoveConnectionString(string name)
        {
            LoadConfigDocument();

            // Retrieve appSettings node
            var node = _xmlDocument.SelectSingleNode("//" + ConnectionStringsSectionName);
            if (node == null) return;

            // Remove 'add' element with corresponding name
            node.RemoveChild(node.SelectSingleNode(string.Format("//add[@name='{0}']", name)));

            // Save changes
            _xmlDocument.Save(_configurationFilePath);
        }

        private void LoadConfigDocument()
        {
            if (_xmlDocument != null) return;

            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(_configurationFilePath);

            Log.DebugFormat("Loaded xml document from: {0}", _configurationFilePath);
        }

        private XmlNode CreateSection(string name)
        {
            var rootNode = _xmlDocument.SelectSingleNode("//configuration");
            var sectionNode = _xmlDocument.CreateElement(name);
            rootNode.AppendChild(sectionNode);

            return sectionNode;
        }
    }
}