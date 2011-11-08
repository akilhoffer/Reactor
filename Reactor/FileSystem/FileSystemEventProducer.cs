﻿using System;
using System.IO;
using Magnum.FileSystem.Internal;
using Stact;

namespace Reactor.FileSystem
{
    /// <summary>
    /// Wraps a FileSystemWatcher as an event producer that sends file system events to the
    /// specified channel
    /// </summary>
    public class FileSystemEventProducer : IDisposable
    {
        private readonly UntypedChannel _channel;
        private readonly string _directory;

        private bool _disposed;
        private FileSystemWatcher _watcher;

        /// <summary>
        /// Creates a FileSystemEventProducer
        /// </summary>
        /// <param name="directory">The directory to watch</param>
        /// <param name="channel">The channel where events should be sent</param>
        public FileSystemEventProducer(string directory, UntypedChannel channel) : this(directory, channel, true) {}

        /// <summary>
        /// Creates a FileSystemEventProducer
        /// </summary>
        /// <param name="directory">The directory to watch</param>
        /// <param name="channel">The channel where events should be sent</param>
        /// <param name="checkSubDirectory">Indicates if subdirectories will be included</param>
        public FileSystemEventProducer(string directory, UntypedChannel channel, bool checkSubDirectory)
        {
            _directory = directory;
            _channel = channel;

            _watcher = new FileSystemWatcher(_directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = checkSubDirectory,
                InternalBufferSize = 8 * 4 * 1024, // 8x the default size
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
                _channel.Send(new FileChangedImpl(e.Name, e.FullPath));
            else if (Directory.Exists(e.FullPath))
                _channel.Send(new DirectoryChangedImpl(e.Name, e.FullPath));
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
                _channel.Send(new FileCreatedImpl(e.Name, e.FullPath));
            else if (Directory.Exists(e.FullPath))
                _channel.Send(new DirectoryCreatedImpl(e.Name, e.FullPath));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            _channel.Send(new FileSystemDeletedImpl(e.Name, e.FullPath));
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (File.Exists(e.FullPath))
                _channel.Send(new FileRenamedImpl(e.Name, e.FullPath, e.OldName, e.OldFullPath));
            else if (Directory.Exists(e.FullPath))
                _channel.Send(new DirectoryRenamedImpl(e.Name, e.FullPath, e.OldName, e.OldFullPath));
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                if (_watcher != null)
                {
                    _watcher.Changed -= OnChanged;
                    _watcher.Created -= OnCreated;
                    _watcher.Deleted -= OnDeleted;
                    _watcher.Renamed -= OnRenamed;

                    _watcher.Dispose();
                    _watcher = null;
                }
            }

            _disposed = true;
        }
    }
}
