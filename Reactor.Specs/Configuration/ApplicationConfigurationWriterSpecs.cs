using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Reactor.Configuration
{
    [TestClass]
    public class ApplicationConfigurationWriter_Tests
    {
        private const string BlankConfigurationFileContents = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration><appSettings></appSettings><connectionStrings></connectionStrings></configuration>";
        private const string TestConfigurationFileContents = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration><appSettings><add key=\"TestSetting\" value=\"TestValue\"/></appSettings><connectionStrings><add name=\"TestConn\" connectionString=\"xxx\"/></connectionStrings></configuration>";
        private const string TestConfigurationFileName = "../../../TestConfiguration.config";
        private const string TestKey = "TestSetting";
        private const string TestValue = "TestValue";

        [TestInitialize]
        public void Initialize()
        {
            if (!File.Exists(TestConfigurationFileName))
                Assert.Fail("Test configuration file not found.");

            // Ensure we have a fresh, blank configuration file.
            File.WriteAllText(TestConfigurationFileName, BlankConfigurationFileContents);
        }

        [TestMethod]
        public void Writes_specified_appSetting_to_an_application_configuration_file()
        {
            var writer = new ApplicationConfigurationWriter(TestConfigurationFileName);
            writer.WriteSetting(TestKey, TestValue);

            Assert.IsTrue(KeyAndValueExist(TestKey, TestValue));
        }

        [TestMethod]
        public void Removes_specified_appSetting_from_an_application_configuration_file()
        {
            File.WriteAllText(TestConfigurationFileName, TestConfigurationFileContents);

            var writer = new ApplicationConfigurationWriter(TestConfigurationFileName);
            writer.RemoveSetting(TestKey);

            Assert.IsFalse(KeyAndValueExist(TestKey, TestValue));
        }

        [TestMethod]
        public void Writes_specified_connectionString_to_an_application_configuration_file()
        {
            var writer = new ApplicationConfigurationWriter(TestConfigurationFileName);
            writer.WriteConnectionString(TestKey, TestValue);

            Assert.IsTrue(ConnectionStringNameAndValueExist(TestKey, TestValue));
        }

        [TestMethod]
        public void Removes_specified_connectionString_from_an_application_configuration_file()
        {
            File.WriteAllText(TestConfigurationFileName, TestConfigurationFileContents);

            var writer = new ApplicationConfigurationWriter(TestConfigurationFileName);
            writer.RemoveSetting(TestKey);

            Assert.IsFalse(ConnectionStringNameAndValueExist(TestKey, TestValue));
        }

        private static bool KeyAndValueExist(string key, string value)
        {
            var doc = new XmlDocument();
            doc.Load(TestConfigurationFileName);

            var node = doc.SelectSingleNode("//appSettings");
            if (node == null) return false;

            var addNode = node.FirstChild;
            if (addNode == null) return false;

            var k = addNode.Attributes["key"].Value;
            var v = addNode.Attributes["value"].Value;

            return (key == k && value == v);
        }

        private static bool ConnectionStringNameAndValueExist(string name, string value)
        {
            var doc = new XmlDocument();
            doc.Load(TestConfigurationFileName);

            var node = doc.SelectSingleNode("//connectionStrings");
            if (node == null) return false;

            var addNode = node.FirstChild;
            if (addNode == null) return false;

            var k = addNode.Attributes["name"].Value;
            var v = addNode.Attributes["connectionString"].Value;

            return (name == k && value == v);
        }
    }
}
// ReSharper restore InconsistentNaming