using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace MultiRPC.Wasm
{
    public class Program
    {
        static LoggingLevelSwitch LoggingLevel;

        private static App _app;

        static async Task<int> Main(string[] args)
        {
            LoggingLevel = new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Debug);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LoggingLevel)
                .WriteTo.ConsoleLogger()
                .CreateLogger();
            
            // Annotate generated DOM elements with x:Name
            Uno.UI.FeatureConfiguration.UIElement.AssignDOMXamlName = true;

            // Annotate generated DOM elements with commonly-used Xaml properties (height/width, alignment etc)
            Uno.UI.FeatureConfiguration.UIElement.AssignDOMXamlProperties = true;

            Application.Start(_ => _app = new App());

            return 0;
        }
    }
}
