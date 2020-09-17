using System;
using Windows.UI.Xaml;
using Microsoft.Extensions.Logging;
using MultiRPC.Shared;

namespace MultiRPC.Wasm
{
    public class Program
    {
        private static App _app;

        static int Main(string[] args)
        {
            Application.Start(_ => _app = new App());

            return 0;
        }
    }
}
