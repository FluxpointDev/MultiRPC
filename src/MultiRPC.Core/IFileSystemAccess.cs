using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Core
{
    public interface IFileSystemAccess : IRequired
    {
        Task<string[]> ReadAllLinesAsync(string path);

        Task<string> ReadAllTextAsync(string path);

        Task<byte[]> ReadAllBytesAsync(string path);

        IEnumerable<string> ReadLines(string path);

        bool CreateDirectory(string path);

        bool DirectoryExists(string path);

        Task<bool> FileExists(string path);

        Task<Stream> GetFileStream(string path, bool writeAccess = false);
    }
}
