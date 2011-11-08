using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Reactor.Composition
{
    public interface IDirectoryBasedComposer
    {
        void Compose(string directoryPath, params object[] attributedParts);
    }

    public class DirectoryBasedComposer : IDirectoryBasedComposer
    {
        public void Compose(string directoryPath, params object[] attributedParts)
        {
            using (var directoryCatalog = new DirectoryCatalog(directoryPath))
            {
                using (var container = new CompositionContainer(directoryCatalog))
                {
                    container.ComposeParts(attributedParts);
                }
            }
        }
    }
}
