using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reactor.Entities;
using Reactor.FileSystem;
using Samurai.Wakizashi.Extensions;
using Samurai.Wakizashi.Workflow;
using Samurai.Wakizashi.Workflow.Fluent;

namespace Reactor.ServiceGrid.Workflows
{
    public class SparkInstanceFactoryWorkflow : SequentialWorkflow<SparkInstanceFactoryWorkflowContext>
    {
        public SparkInstanceFactoryWorkflow(IFileSystem fileSystem)
        {
            Context = new SparkInstanceFactoryWorkflowContext
            {
                FileSystem = fileSystem
            };

            Configure(RegisterWorkflowSteps);
        }

        private void RegisterWorkflowSteps(ISequentialWorkflow<SparkInstanceFactoryWorkflowContext> w)
        {
            w.Do(ResolveSparkConfigurationDirectory).OnThread(ThreadType.Task).Done();
            w.Do(DiscoverSerializedSparks).Done();
            w.Do(HydrateSparkInstances).Done();
        }

        private void ResolveSparkConfigurationDirectory()
        {
            Context.SparkConfigurationDirectoryPath = Path.Combine(Environment.CurrentDirectory, "Sparks");

            if(!Context.FileSystem.DirectoryExists(Context.SparkConfigurationDirectoryPath))
                Context.FileSystem.CreateDirectory(Context.SparkConfigurationDirectoryPath);
        }

        private void DiscoverSerializedSparks()
        {
            Context.SerializedSparks = Context.FileSystem.GetAllFiles(Context.SparkConfigurationDirectoryPath, "*.xml");
        }

        private void HydrateSparkInstances()
        {
            var sparks = new List<ISpark>(Context.SerializedSparks.Count());

            foreach (var serializedSpark in Context.SerializedSparks)
            {
                try
                {
                    sparks.Add(CreateSparkFromXml(serializedSpark));
                }
                catch (Exception ex)
                {
                    Logger.Error("Skipping spark hydration due to creation failure. See inner exception for details.", ex);
                }   
            }

            Context.SparkInstances = sparks.ToArray();
        }

        private ISpark CreateSparkFromXml(string xmlFilePath)
        {
            var xml = Context.FileSystem.ReadAllText(xmlFilePath);
            var serializedSpark = ObjectExtensions.FromXml<SerializedSpark>(xml);
            var sparkType = Type.GetType(serializedSpark.FullyQualifiedSparkType);

            return (ISpark)ObjectExtensions.FromXml(sparkType, serializedSpark.SparkInstanceXml);
        }
    }

    public class SparkInstanceFactoryWorkflowContext : IWorkflowContext
    {
        public IFileSystem FileSystem { get; set; }
        public IEnumerable<string> SerializedSparks { get; set; }
        public string SparkConfigurationDirectoryPath { get; set; }
        public IEnumerable<ISpark> SparkInstances { get; set; }
    }
}
