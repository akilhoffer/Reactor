using System.Collections.Generic;
using System.IO;

namespace Reactor.FileSystem
{
    public interface IFileSystem
    {
        bool DirectoryExists(string directoryPath);
        void CreateDirectory(string directoryToCreate);
        void DeleteDirectory(string directoryToDelete);
        void DeleteDirectory(string directoryToDelete, bool recursive);
        bool FileExists(string filePath);
        void CopyFile(string originalPath, string destinationPath);
        void CopyFile(string originalPath, string destinationPath, bool overwrite);
        void WriteTextToNewFile(string filePath, string textToWrite);
        IEnumerable<string> GetAllFiles(string path, string searchPattern);
        string ReadAllText(string filePath);
    }

    public class PhysicalFileSystem : IFileSystem
    {
        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public void CreateDirectory(string directoryToWatch)
        {
            Directory.CreateDirectory(directoryToWatch);
        }

        public void DeleteDirectory(string directoryToDelete)
        {
            Directory.Delete(directoryToDelete);
        }

        public void DeleteDirectory(string directoryToDelete, bool recursive)
        {
            Directory.Delete(directoryToDelete, recursive);
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public void CopyFile(string originalPath, string destinationPath)
        {
            CopyFile(originalPath, destinationPath, false);
        }

        public void CopyFile(string originalPath, string destinationPath, bool overwrite)
        {
            File.Copy(originalPath, destinationPath, overwrite);
        }

        public void WriteTextToNewFile(string filePath, string textToWrite)
        {
            // TODO: Not optimal to create and close. Come back here and have the contents written while the filestream is still open.
            File.Create(filePath).Close();
            File.WriteAllText(filePath, textToWrite);
        }

        public IEnumerable<string> GetAllFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern);
        }

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
