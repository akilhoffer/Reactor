using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reactor.Composition;
using Reactor.FileSystem;
using Reactor.Specs;

namespace Reactor.Customization
{
    public class CustomizationManagerBaseSpecs : SpecificationFor<CustomizationManagerBase>
    {
        protected IDirectoryBasedComposer Composer;
        protected IFileSystem FileSystem;

        public override void Context()
        {
            Composer = new Mock<IDirectoryBasedComposer>().Object;
            FileSystem = new Mock<IFileSystem>().Object;
        }

        public override CustomizationManagerBase InitializeSubject()
        {
            return new InitializationCustomizationManager(Composer, FileSystem);
        }

        [TestClass]
        public class WhenAnInvalidDirectoryPathIsSupplied : CustomizationManagerBaseSpecs
        {
            public override void Because()
            {
                Mock<IFileSystem>.Get(FileSystem)
                    .Setup(fs => fs.DirectoryExists(It.IsAny<string>()))
                    .Returns(false);
            }

            [TestMethod]
            [ExpectedException(typeof(DirectoryNotFoundException))]
            public void ThrowsAnException()
            {
                Subject.DiscoverCustomizers("C:\\DIRECTORYTHATDOESNOTEXIST");
            }
        }
    }
}
