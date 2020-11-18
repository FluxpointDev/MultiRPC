using MultiRPC.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Uno;
using Uno.Foundation;
using Uno.Threading;
using Uno.UI.Wasm;

namespace MultiRPC.Wasm
{
    class FileSystemAccess : IFileSystemAccess
    {
        private string baseUri;
        HttpClient HttpClient = new HttpClient(new WasmHttpHandler());

        //TODO: Add some kind of caching, can see this helping with perf
        //TODO: Add remaining stuff
        public FileSystemAccess()
        {
            var applicationUri = WebAssemblyRuntime.InvokeJS("window.location.href");
            if (applicationUri[applicationUri.Length - 1] != '/')
            {
                applicationUri += '/';
            }
            var packageID = WebAssemblyRuntime.InvokeJS("config.environmentVariables[\"UNO_BOOTSTRAP_APP_BASE\"]");
            baseUri = applicationUri + packageID;
            Log.Logger.Debug($"Uri for grabbing files: {baseUri}");
        }

        public bool CreateDirectory(string path) => throw new NotImplementedException();

        public bool DirectoryExists(string path) => throw new NotImplementedException();

        //TODO: Check if it is a file and not dir
        public async Task<bool> FileExists(string path) => throw new NotImplementedException();

        public async Task<byte[]> ReadAllBytesAsync(string path) => throw new NotImplementedException();

        public async Task<string[]> ReadAllLinesAsync(string path) => throw new NotImplementedException();

        public async Task<string> ReadAllTextAsync(string path)
        {
            var stream = await GetFileStreamAsync(path);
            StreamReader reader = new StreamReader(stream);
            var cont = await reader.ReadToEndAsync();

            return cont;
        }

        public IEnumerable<string> ReadLines(string path) => throw new NotImplementedException();

        public async Task<Stream> GetFileStreamAsync(string path, bool writeAccess = false)
        {
            //TODO: Check if they are writing to the WebAssembly DB and not attemping to write to disk
            if (writeAccess)
            {
                throw new NotSupportedException("Unable to write to streams");
            }

            //Make sure that the path is all good to be used
            if (path[0] != '/')
            {
                path = '/' + path;
            }
            path.Replace(Path.DirectorySeparatorChar, '/');

            Stream stream;
            try
            {
                stream = await HttpClient.GetStreamAsync(baseUri + path);
                if (stream == null || stream == Stream.Null)
                {
                    Log.Logger.Error("Stream came back as Null or it has no backing");
                    return Stream.Null;
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error("Unable to get Stream");
                Log.Logger.Error(e.Message);
                return Stream.Null;
            }

            Log.Logger.Debug($"Got Stream");
            return stream;
        }
    }
}
