using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reactor.Configuration;
using Reactor.Entities;
using Reactor.ServiceGrid.Workflows;
using Reactor.Specs;
using Samurai.Wakizashi.Workflow;

// ReSharper disable CheckNamespace
namespace Reactor.ServiceGrid
{
    public class StreamServiceSpecs : SpecificationFor<StreamService>
    {
        protected ISequentialWorkflow<SparkInstanceFactoryWorkflowContext> SparkFactoryWorkflow;
        protected ISpark TestSpark;

        public override StreamService InitializeSubject()
        {
            return new TestStreamService(SparkFactoryWorkflow, new Mock<IConfigurationAggregator>().Object);
        }

        public override void Context()
        {
            TestSpark = new Mock<ISpark>().Object;

            var mockWorkflow = new Mock<ISequentialWorkflow<SparkInstanceFactoryWorkflowContext>>();
            mockWorkflow.SetupGet(w => w.Context).Returns(new SparkInstanceFactoryWorkflowContext
            {
                SparkInstances = new List<ISpark> { TestSpark }
            });
            SparkFactoryWorkflow = mockWorkflow.Object;
        }

        public override void Because()
        {
            Action<WorkflowCompletedEventArgs<SparkInstanceFactoryWorkflowContext>> completedAction = null;

            Mock.Get(SparkFactoryWorkflow)
                .Setup(w => w.Start())
                .Callback(() =>
                {
                    if (completedAction != null)
                        completedAction.Invoke(null);
                });

            Mock.Get(SparkFactoryWorkflow)
                .Setup(w => w.OnCompleted(It.IsAny<Action<WorkflowCompletedEventArgs<SparkInstanceFactoryWorkflowContext>>>()))
                .Callback((Action<WorkflowCompletedEventArgs<SparkInstanceFactoryWorkflowContext>> action) => completedAction = action);

            Subject.Start();
        }

        [TestClass]
        public class WhenStarted : StreamServiceSpecs
        {
            [TestMethod]
            public void DiscoversDeclaredSparks()
            {
                Assert.IsTrue(Subject.SparkInstances.Any());
            }

            [TestMethod]
            public void StartsAllDiscoveredSparkInstances()
            {
                Mock.Get(TestSpark)
                    .Verify(s => s.Start(), Times.Exactly(1));
            }
        }

        [TestClass]
        public class WhenStoped : StreamServiceSpecs
        {
            public override void Because()
            {
                base.Because();

                Subject.Stop();
            }

            [TestMethod]
            public void StopsAllRunningSparkInstances()
            {
                Mock.Get(TestSpark)
                    .Verify(s => s.Stop(), Times.Exactly(1));
            }
        }
    }

    internal class TestStreamService : StreamService
    {
        public TestStreamService(ISequentialWorkflow<SparkInstanceFactoryWorkflowContext> sparkFactoryWorkflow, IConfigurationAggregator configurationAggregator) : base(sparkFactoryWorkflow, configurationAggregator)
        {
            
        }
    }
}
// ReSharper restore CheckNamespace