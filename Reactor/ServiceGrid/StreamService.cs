using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reactor.Configuration;
using Reactor.Entities;
using Reactor.FileSystem;
using Reactor.ServiceGrid.Workflows;
using Samurai.Wakizashi.Workflow;

namespace Reactor.ServiceGrid
{
    public abstract class StreamService : ReactorServiceBase
    {
        private readonly ISequentialWorkflow<SparkInstanceFactoryWorkflowContext> _sparkFactoryWorkflow;
        private readonly ConcurrentBag<ISpark> _discoveredSparks = new ConcurrentBag<ISpark>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactorServiceBase"/> class.
        /// </summary>
        /// <param name="configurationAggregator">The configuration aggregator.</param>
        protected StreamService(IConfigurationAggregator configurationAggregator)
            : this(new SparkInstanceFactoryWorkflow(new PhysicalFileSystem()), configurationAggregator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactorServiceBase"/> class.
        /// </summary>
        /// <param name="sparkFactoryWorkflow">The spark factory workflow.</param>
        /// <param name="configurationAggregator">The configuration aggregator.</param>
        protected StreamService(ISequentialWorkflow<SparkInstanceFactoryWorkflowContext> sparkFactoryWorkflow, IConfigurationAggregator configurationAggregator) : base(configurationAggregator)
        {
            if (sparkFactoryWorkflow == null) throw new ArgumentNullException("sparkFactoryWorkflow");

            _sparkFactoryWorkflow = sparkFactoryWorkflow;
        }

        protected override void OnStarting()
        {
            DiscoverConfiguredSparkInstances();
        }

        protected override void OnShuttingDown()
        {
            StopSparkInstances();
        }

        private void StopSparkInstances()
        {
            foreach (var sparkInstance in SparkInstances)
            {
                sparkInstance.Stop();
            }
        }

        public IEnumerable<ISpark> SparkInstances
        {
            get { return _discoveredSparks.ToArray(); }
        }

        private void DiscoverConfiguredSparkInstances()
        {
            _sparkFactoryWorkflow.OnFailed(args =>
            {
                Log.FatalFormat("StreamService cannot start because it failed to properly load Spark Instances. Reason: {0}", args.Exception.Message);
                Stop();
            });
            _sparkFactoryWorkflow.OnCompleted(args =>
            {
                foreach (var sparkInstance in _sparkFactoryWorkflow.Context.SparkInstances)
                _discoveredSparks.Add(sparkInstance);

                StartDiscoveredSparks();
            });
            _sparkFactoryWorkflow.Start();
        }

        private void StartDiscoveredSparks()
        {
            foreach (var discoveredSpark in _discoveredSparks)
            {
                try
                {
                    discoveredSpark.Start();
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Unable to start Spark instance: {0}. \r\n\tDetails: {1}", discoveredSpark.Name, e);
                }
            }
        }
    }
}
