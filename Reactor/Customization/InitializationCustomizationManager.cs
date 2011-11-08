using System.Linq;
using Reactor.Composition;
using Reactor.FileSystem;
using log4net;

namespace Reactor.Customization
{
    public class InitializationCustomizationManager : CustomizationManagerBase
    {
        private static ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationCustomizationManager"/> class.
        /// </summary>
        public InitializationCustomizationManager(IDirectoryBasedComposer composer, IFileSystem fileSystem) : base(composer, fileSystem)
        {
            _log = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Runs all customers found by this instance.
        /// </summary>
        public override void RunAll()
        {
            // Sort customizers by execution order
            Customizers = Customizers.OrderBy(i => i.ExecutionOrder).ToList();

            foreach (var item in Customizers)
            {
                if (_log.IsDebugEnabled) _log.DebugFormat("Executing initialization customizer: {0}", item.GetType().FullName);
                item.InitializeReactor();
            }
        }
    }
}
