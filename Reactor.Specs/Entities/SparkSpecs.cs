using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reactor.Specs;
using FluentAssertions;

// ReSharper disable CheckNamespace
namespace Reactor.Entities
// ReSharper restore CheckNamespace
{
    public class SparkSpecs : SpecificationFor<Spark>
    {
        public override Spark InitializeSubject()
        {
            return new Mock<Spark>().Object;
        }

        [TestClass]
        public class WhenStarted : SparkSpecs
        {
            private bool _transitionedToStartingState;
            private bool _transitionedToStartedState;

            public override void Because()
            {
                Subject.StateChanged += (o, e) =>
                {
                    switch (e.CurrentState)
                    {
                        case RunState.Starting:
                            _transitionedToStartingState = true;
                            break;
                        case RunState.Started:
                            _transitionedToStartedState = true;
                            break;
                    }
                };

                Subject.FireConditionCheckInterval = TimeSpan.FromMilliseconds(5);
                Subject.Start();
            }

            public override void TearDown()
            {
                Subject.Stop();
            }

            [TestMethod]
            public void StateTransitionsToStarting()
            {
                _transitionedToStartingState.Should().BeTrue();
            }

            [TestMethod]
            public void StateTransitionsToStarted()
            {
                _transitionedToStartedState.Should().BeTrue();
            }

            [TestMethod]
            public void BeginsPollingFireCheckMethod()
            {
                var mock = Mock.Get(Subject);
                mock.Verify(s => s.ShouldFireReaction());
            }
        }

        [TestClass]
        public class WhenStopped : SparkSpecs
        {
            private bool _transitionedToStoppingState;
            private bool _transitionedToStoppedState;

            public override void Because()
            {
                Subject.StateChanged += (o, e) =>
                {
                    switch (e.CurrentState)
                    {
                        case RunState.Stopping:
                            _transitionedToStoppingState = true;
                            break;
                        case RunState.Stopped:
                            _transitionedToStoppedState = true;
                            break;
                    }
                };

                Subject.FireConditionCheckInterval = TimeSpan.FromMilliseconds(500);
                Subject.Start();
                Subject.Stop();
            }

            [TestMethod]
            public void StateTransitionsToStopping()
            {
                _transitionedToStoppingState.Should().BeTrue();
            }

            [TestMethod]
            public void StateTransitionsToStopped()
            {
                _transitionedToStoppedState.Should().BeTrue();
            }

            [TestMethod]
            public void StopsPollingFireCheckMethod()
            {
                var mock = Mock.Get(Subject);
                mock.Verify(s => s.ShouldFireReaction(), Times.Never());
            }
        }

        [TestClass]
        public class WhileRunning : SparkSpecs
        {
            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            public override void Because()
            {
                Subject.FireConditionCheckInterval = TimeSpan.FromMilliseconds(10);
            }

            public override void TearDown()
            {
                Subject.Stop();
            }

            [TestMethod]
            public void ExecutesReactionWhenShouldFireIsIndicated()
            {
                var mock = Mock.Get(Subject);

                mock.Setup(s => s.ShouldFireReaction()).Returns(true);
                mock.Setup(s => s.React()).Callback(() => _autoResetEvent.Set());
                

                Subject.Start();
                _autoResetEvent.WaitOne(TimeSpan.FromSeconds(1));
                Subject.Stop();

                mock.VerifyAll();
            }

            [TestMethod]
            public void DoesNotExecuteReactionWhenShouldFireIsNotIndicated()
            {
                var mock = Mock.Get(Subject);

                mock.Setup(s => s.ShouldFireReaction()).Returns(false);
                Subject.Start();
                Thread.Sleep(25);
                Subject.Stop();

                mock.Verify(s => s.React(), Times.Never());
            }
        }

        [TestClass]
        public class WhenFireConditionCheckFails : SparkSpecs
        {
            private bool _transitionedToFaultedState;

            public override void Because()
            {
                Subject.FireConditionCheckInterval = TimeSpan.FromMilliseconds(50);
                Mock.Get(Subject)
                    .Setup(s => s.ShouldFireReaction())
                    .Throws<InvalidOperationException>();
                Subject.StateChanged += (o, e) =>
                                            {
                                                _transitionedToFaultedState = true;
                                            };
                Subject.Start();
            }

            public override void TearDown()
            {
                Subject.Stop();
            }

            [TestMethod]
            public void TransitionsToFaultedState()
            {
                Assert.IsTrue(_transitionedToFaultedState);
            }
        }
    }
}
