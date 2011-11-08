using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reactor.FileSystem;
using Reactor.Specs;

// ReSharper disable CheckNamespace
namespace Reactor.ServiceGrid.Workflows
{
    public class SparkInstanceFactoryWorkflowSpecs : SpecificationFor<SparkInstanceFactoryWorkflow>
    {
        protected IFileSystem FileSystem;
        protected AutoResetEvent ResetEvent = new AutoResetEvent(false);
        protected string SparkDirectoryPath = Path.Combine(Environment.CurrentDirectory, "Sparks");
        protected string SerializedSparkPath = "C:\\Spark1.xml";
        protected string SerializedSpark = Specs.Properties.Resources.SerializedSparkXml;

        public override void Context()
        {
            FileSystem = new Mock<IFileSystem>().Object;

            Mock.Get(FileSystem)
                .Setup(fs => fs.DirectoryExists(SparkDirectoryPath))
                .Returns(false);

            Mock.Get(FileSystem)
                .Setup(fs => fs.GetAllFiles(SparkDirectoryPath, "*.xml"))
                .Returns(new[] { SerializedSparkPath });

            Mock.Get(FileSystem)
                .Setup(fs => fs.ReadAllText(SerializedSparkPath))
                .Returns(SerializedSpark);
        }

        public override SparkInstanceFactoryWorkflow InitializeSubject()
        {
            return new SparkInstanceFactoryWorkflow(FileSystem);
        }

        [TestClass]
        public class WhenStarted : SparkInstanceFactoryWorkflowSpecs
        {
            public override void Because()
            {
                Subject.Start();
                Subject.OnCompleted(a => ResetEvent.Set());

                ResetEvent.WaitOne(TimeSpan.FromSeconds(10));
            }

            [TestMethod]
            public void ResolvesSparkConfigurationDirectory()
            {
                // Assert that the context contains the spark directory path
                Assert.IsFalse(string.IsNullOrEmpty(Subject.Context.SparkConfigurationDirectoryPath));

                // Assert that spark directory was created
                Mock.Get(FileSystem)
                    .Verify(fs => fs.CreateDirectory(SparkDirectoryPath));
            }

            [TestMethod]
            public void DiscoversSerializedSparks()
            {
                Assert.IsTrue(Subject.Context.SerializedSparks.Contains(SerializedSparkPath));
            }

            [TestMethod]
            public void HydratesDiscoveredSparkInstances()
            {
                Assert.IsNotNull(Subject.Context.SparkInstances);
                Assert.IsTrue(Subject.Context.SparkInstances.Any());
            }
        }
    }
}
// ReSharper restore CheckNamespace