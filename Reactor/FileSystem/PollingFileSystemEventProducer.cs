using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Magnum.Extensions;
using Magnum.FileSystem.Events;
using Magnum.FileSystem.Internal;
using MassTransit.Util;
using Stact;
using log4net;

namespace Reactor.FileSystem
{
    public class PollingFileSystemEventProducer : IDisposable
    {
        #region Fields

        private readonly UntypedChannel _channel;
        private readonly TimeSpan _checkInterval;
        private readonly ChannelConnection _connection;
        private readonly string _directory;
        private readonly Fiber _fiber;
        private readonly FileSystemEventProducer _fileSystemEventProducer;
        private readonly Dictionary<string, Guid> _hashes;
        private readonly Scheduler _scheduler;
        private bool _disposed;
        private ScheduledOperation _scheduledAction;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PollingFileSystemEventProducer));

        #endregion

        /// <summary>
        /// Creates a PollingFileSystemEventProducer
        /// </summary>		
        /// <param name="directory">The directory to watch</param>
        /// <param name="channel">The channel where events should be sent</param>
        /// <param name="scheduler">Event scheduler</param>
        /// <param name="fiber">Fiber to schedule on</param>
        /// <param name="checkInterval">The maximal time between events or polls on a given file</param>
        public PollingFileSystemEventProducer(string directory, UntypedChannel channel, Scheduler scheduler, Fiber fiber, TimeSpan checkInterval) : this(directory, channel, scheduler, fiber, checkInterval, true)
        {}

        /// <summary>
        /// Creates a PollingFileSystemEventProducer
        /// </summary>		
        /// <param name="directory">The directory to watch</param>
        /// <param name="channel">The channel where events should be sent</param>
        /// <param name="scheduler">Event scheduler</param>
        /// <param name="fiber">Fiber to schedule on</param>
        /// <param name="checkInterval">The maximal time between events or polls on a given file</param>
        /// <param name="checkSubDirectory">Indicates if subdirectorys will be checked or ignored</param>
        public PollingFileSystemEventProducer(string directory, UntypedChannel channel, [NotNull] Scheduler scheduler, Fiber fiber, TimeSpan checkInterval, bool checkSubDirectory)
        {
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            _directory = directory;
            _channel = channel;
            _fiber = fiber;
            _hashes = new Dictionary<string, Guid>();
            _scheduler = scheduler;
            _checkInterval = checkInterval;

            _scheduledAction = scheduler.Schedule(3.Seconds(), _fiber, HashFileSystem);

            var myChannel = new ChannelAdapter();

            _connection = myChannel.Connect(connectionConfigurator =>
            {
                connectionConfigurator.AddConsumerOf<FileSystemChanged>().UsingConsumer(HandleFileSystemChangedAndCreated);
                connectionConfigurator.AddConsumerOf<FileSystemCreated>().UsingConsumer(HandleFileSystemChangedAndCreated);
                connectionConfigurator.AddConsumerOf<FileSystemRenamed>().UsingConsumer(HandleFileSystemRenamed);
                connectionConfigurator.AddConsumerOf<FileSystemDeleted>().UsingConsumer(HandleFileSystemDeleted);
            });

            _fileSystemEventProducer = new FileSystemEventProducer(directory, myChannel, checkSubDirectory);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void HandleFileSystemChangedAndCreated(FileSystemEvent fileEvent)
        {
            HandleHash(fileEvent.Path, GenerateHashForFile(fileEvent.Path));
        }

        void HandleFileSystemRenamed(FileSystemRenamed fileEvent)
        {
            HandleFileSystemChangedAndCreated(fileEvent);
            HandleFileSystemDeleted(new FileSystemDeletedImpl(fileEvent.OldName, fileEvent.OldPath));
        }

        void HandleFileSystemDeleted(FileSystemEvent fileEvent)
        {
            RemoveHash(fileEvent.Path);
        }

        void HandleHash(string key, Guid newHash)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (_hashes.ContainsKey(key))
            {
                if (_hashes[key] != newHash)
                {
                    _hashes[key] = newHash;
                    _channel.Send(new FileChangedImpl(Path.GetFileName(key), key));
                }
            }
            else
            {
                _hashes.Add(key, newHash);
                _channel.Send(new FileCreatedImpl(Path.GetFileName(key), key));
            }
        }

        void RemoveHash(string key)
        {
            _hashes.Remove(key);
            _channel.Send(new FileSystemDeletedImpl(Path.GetFileName(key), key));
        }

        void HashFileSystem()
        {
            try
            {
                var newHashes = new Dictionary<string, Guid>();

                ProcessDirectory(newHashes, _directory);

                // process all the new hashes found
                newHashes.ToList().ForEach(x => HandleHash(x.Key, x.Value));

                // remove any hashes we couldn't process
                _hashes.Where(x => !newHashes.ContainsKey(x.Key)).ToList().ConvertAll(x => x.Key).ForEach(RemoveHash);
            }
            finally
            {
                _scheduledAction = _scheduler.Schedule(_checkInterval, _fiber, HashFileSystem);
            }
        }

        void ProcessDirectory(Dictionary<string, Guid> hashes, string baseDirectory)
        {
            string[] files = Directory.GetFiles(baseDirectory);

            foreach (string file in files)
            {
                string fullFileName = Path.Combine(baseDirectory, file);
                hashes.Add(fullFileName, GenerateHashForFile(fullFileName));
            }

            foreach (var directory in Directory.GetDirectories(baseDirectory).ToList())
                ProcessDirectory(hashes, directory);
        }

        static Guid GenerateHashForFile(string file)
        {
            try
            {
                string hashValue;
                using (FileStream f = File.OpenRead(file))
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    byte[] fileHash = md5.ComputeHash(f);

                    hashValue = BitConverter.ToString(fileHash).Replace("-", "");
                }

                return new Guid(hashValue);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Problem hashing file '{0}'. See the next log message for details.", file);

                // chew up exception and say empty hash
                // can we do something more interesting than this?
                // Henrik: perhaps log it so that we can abduce a better exception handling policy in the future?
                Logger.Error("Problem creating MD5 hash of file '{0}'. See inner exception details (have a look at the "
                    + "piece of code generating this to have a look at whether we can do something better).", e);

                return Guid.Empty;
            }
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _scheduledAction.Cancel();
                _connection.Dispose();
                _fileSystemEventProducer.Dispose();
            }

            _disposed = true;
        }
    }
}
