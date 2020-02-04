using System.IO;

namespace MultiRPC.Core
{
    /// <summary>
    /// <see cref="FileWatch"/> helper
    /// </summary>
    public class FileWatch : FileSystemWatcher
    {
        /// <inheritdoc cref="FileSystemWatcher"/>
        /// <param name="file">File to check</param>
        /// <param name="folder">Folder to check</param>
        /// <param name="fileExtension">Files with a certain file extension to be checked</param>
        public FileWatch(string file = null, string folder = null, string fileExtension = null) 
            : base(FileLocations.ConfigFolder, file ?? folder ?? fileExtension)
        {
            this.EnableRaisingEvents = true;
        }
    }
}