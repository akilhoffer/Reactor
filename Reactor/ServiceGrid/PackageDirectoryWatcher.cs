using System;
using Magnum.Extensions;
using Magnum.FileSystem;
using Magnum.FileSystem.Events;
using Magnum.FileSystem.Zip;
using Reactor.Exceptions;
using Reactor.FileSystem;
using Stact;
using Stact.Internal;

namespace Reactor.ServiceGrid
{
    internal class PackageDirectoryWatcher
    {
        private readonly IFileSystem _fileSystem;
        private Action<Directory> _actionToTake;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDirectoryWatcher"/> class using the 
        /// default implementations of required dependencies.
        /// </summary>
        public PackageDirectoryWatcher() : this(new PhysicalFileSystem()) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDirectoryWatcher"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public PackageDirectoryWatcher(IFileSystem fileSystem)
        {
            if (fileSystem == null) throw new ArgumentNullException("fileSystem");
            _fileSystem = fileSystem;
        }

        public IDisposable Watch(string directoryToWatch, Action<Directory> actionToTake)
        {
            if (actionToTake == null) throw new ArgumentNullException("actionToTake");

            _actionToTake = actionToTake;

            if (!_fileSystem.DirectoryExists(directoryToWatch))
                _fileSystem.CreateDirectory(directoryToWatch);

            Func<Fiber> fiberFactory = () => new SynchronousFiber();
            UntypedChannel eventChannel = new ChannelAdapter();
            eventChannel.Connect(x => x.AddConsumerOf<FileCreated>().UsingConsumer(ProcessNewFile));

            Scheduler scheduler = new TimerScheduler(fiberFactory());
            var watcher = new PollingFileSystemEventProducer(directoryToWatch, eventChannel, scheduler, fiberFactory(), 1.Seconds());

            return watcher;
        }

        private void ProcessNewFile(FileCreated message)
        {
            if (!message.Path.EndsWith("zip")) return;

            Directory dir = new ZipFileDirectory(PathName.GetPathName(message.Path));
            try
            {
                _actionToTake(dir);
            }
            catch (Exception ex)
            {
                string msg = "There was an error processing the stream package '{0}'".FormatWith(message.Path);
                throw new StreamPackageException(msg, ex);
            }
        }
    }
}
