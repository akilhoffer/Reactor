using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Reactor.Composition;
using Reactor.FileSystem;
using log4net;

namespace Reactor.Customization
{
    public interface ICustomizationManager
    {
        /// <summary>
        /// Runs all customers found by this instance.
        /// </summary>
        void RunAll();

        void DiscoverCustomizers(string directoryPath);

        [ImportMany(AllowRecomposition = true)]
        IEnumerable<ICustomizeReactorInitialization> Customizers { get; }
    }

    public abstract class CustomizationManagerBase : ICustomizationManager
    {
        private readonly IDirectoryBasedComposer _composer;
        private readonly IFileSystem _fileSystem;
        protected static ILog Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomizationManagerBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="composer">The composer.</param>
        /// <param name="fileSystem">The file system.</param>
        protected CustomizationManagerBase(IDirectoryBasedComposer composer, IFileSystem fileSystem)
        {
            if (composer == null) throw new ArgumentNullException("composer");
            if (fileSystem == null) throw new ArgumentNullException("fileSystem");

            _composer = composer;
            _fileSystem = fileSystem;
            Log = LogManager.GetLogger(GetType());

            Customizers = new List<ICustomizeReactorInitialization>();

            if (Log.IsDebugEnabled) Log.DebugFormat("There were {0} customizers found", Customizers.Count());
        }

        public void DiscoverCustomizers(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentNullException("directoryPath");

            if (!_fileSystem.DirectoryExists(directoryPath))
                throw new DirectoryNotFoundException(directoryPath);

            _composer.Compose(directoryPath, this);
        }

        /// <summary>
        /// Runs all customers found by this instance.
        /// </summary>
        public abstract void RunAll();

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<ICustomizeReactorInitialization> Customizers { get; protected set; }
    }
}
