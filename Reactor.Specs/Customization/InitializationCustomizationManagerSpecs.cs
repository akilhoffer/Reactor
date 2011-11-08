using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reactor.Composition;
using Reactor.Customization;
using Reactor.FileSystem;

namespace Reactor.Specs.Customization
{
    public class InitializationCustomizationManagerSpecs : SpecificationFor<InitializationCustomizationManager>
    {
        #region Fields

        protected IDirectoryBasedComposer Composer;
        protected IFileSystem FileSystem;
        protected ICustomizeReactorInitialization Customizer1;
        protected ICustomizeReactorInitialization Customizer2;
        protected bool Customizer1Executed;
        protected bool Customizer2Executed;

        #endregion

        public override InitializationCustomizationManager InitializeSubject()
        {
            Composer = new Mock<IDirectoryBasedComposer>().Object;
            FileSystem = new Mock<IFileSystem>().Object;

            return new InitializationCustomizationManager(Composer, FileSystem);
        }

        [TestClass]
        public class WhenProvidedAValidDirectoryPath_AndCustomizersAreProvided : InitializationCustomizationManagerSpecs
        {
            public override void Context()
            {
                var mock1 = new Mock<ICustomizeReactorInitialization>();
                var mock2 = new Mock<ICustomizeReactorInitialization>();
                Customizer1 = mock1.Object;
                Customizer2 = mock2.Object;

                mock1.Setup(c => c.InitializeReactor()).Callback(() => Customizer1Executed = true);
                mock2.Setup(c => c.InitializeReactor()).Callback(() => Customizer2Executed = true);
            }

            public override void Because()
            {
                var customizersList = (List<ICustomizeReactorInitialization>) Subject.Customizers;

                customizersList.Add(Customizer1);
                customizersList.Add(Customizer2);

                Subject.RunAll();
            }

            [TestMethod]
            public void RunsAllCustomizersProvided()
            {
                Assert.IsTrue(Customizer1Executed);
                Assert.IsTrue(Customizer2Executed);
            }
        }
    }
}
