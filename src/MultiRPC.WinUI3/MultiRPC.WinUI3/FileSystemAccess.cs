using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.WinUI3
{
    class FileSystemAccess : IFileSystemAccess
    {
        public bool CreateDirectory(string path) => Directory.CreateDirectory(GetFullPath(path)).Exists;

        public bool DirectoryExists(string path) => Directory.Exists(GetFullPath(path));

        public async Task<bool> FileExists(string path) => File.Exists(GetFullPath(path));

        public async Task<Stream> GetFileStreamAsync(string path, bool writeAccess = false) => writeAccess ? File.OpenRead(GetFullPath(path)) : File.OpenRead(GetFullPath(path));

        public async Task<byte[]> ReadAllBytesAsync(string path) => await File.ReadAllBytesAsync(GetFullPath(path));

        public async Task<string[]> ReadAllLinesAsync(string path) => await File.ReadAllLinesAsync(GetFullPath(path));

        private string GetFullPath(string path)
        {
            var ass = System.Reflection.Assembly.GetExecutingAssembly();
            var folder = Path.GetDirectoryName(ass.Location);
            folder = folder.Remove(folder.LastIndexOf(Path.GetFileName(folder)));
            return Path.Combine(folder, path);
        }

        public async Task<string> ReadAllTextAsync(string path) => await File.ReadAllTextAsync(GetFullPath(path));

        public IEnumerable<string> ReadLines(string path) => File.ReadLines(GetFullPath(path));
    }
}
