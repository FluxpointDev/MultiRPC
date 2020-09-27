using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MultiRPC.UWP
{
    class FileSystemAccess : IFileSystemAccess
    {
        //TODO: Add some kind of caching, can see this helping with perf
        //TODO: Add remaining stuff

        public bool CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        private async Task<StorageFolder> GetFolder(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                path = Path.Combine(root, Path.GetDirectoryName(path));
            }

            try
            {
                var sampleFile =
                   await StorageFolder.GetFolderFromPathAsync(path).AsTask().ConfigureAwait(false);
                return sampleFile;
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e.Message);
            }

            return null;
        }

        private async Task<StorageFile> GetFile(string path)
        {
            var folder = await GetFolder(path).ConfigureAwait(false);
            if (folder == null)
            {
                return null;
            }

            var fileName = Path.GetFileName(path);
            try
            {
                var file = await folder.GetFileAsync(fileName).AsTask().ConfigureAwait(false);
                return file;
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e.Message);
            }

            return null;
        }

        public async Task<bool> FileExists(string path) => IsFileAvailable(await GetFile(path).ConfigureAwait(false));

        private bool IsFileAvailable(StorageFile file) => file != null && file.IsAvailable;

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            var file = await GetFile(path).ConfigureAwait(false);
            if (!IsFileAvailable(file))
            {
                return null;
            }

            var buffer = await FileIO.ReadBufferAsync(file).AsTask().ConfigureAwait(false);
            byte[] contents = null;
            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                contents = new byte[dataReader.UnconsumedBufferLength];
                dataReader.ReadBytes(contents);
            }
            return contents;
        }

        public async Task<string[]> ReadAllLinesAsync(string path)
        {
            var file = await GetFile(path).ConfigureAwait(false);
            if (!IsFileAvailable(file))
            {
                return null;
            }

            return (await FileIO.ReadLinesAsync(file).AsTask().ConfigureAwait(false)).ToArray();
        }

        public async Task<string> ReadAllTextAsync(string path)
        {
            var file = await GetFile(path).ConfigureAwait(false);
            if (!IsFileAvailable(file))
            {
                return null;
            }

            var content = await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false);
            return content;
        }

        public IEnumerable<string> ReadLines(string path)
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> GetFileStream(string path, bool writeAccess = false)
        {
            var file = await GetFile(path).ConfigureAwait(false);
            if (!IsFileAvailable(file))
            {
                return Stream.Null;
            }

            return await (writeAccess ? 
                file.OpenStreamForWriteAsync() :
                file.OpenStreamForReadAsync()).ConfigureAwait(false);
        }
    }
}
