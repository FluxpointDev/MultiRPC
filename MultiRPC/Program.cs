using System;
using System.IO;
using Avalonia;
using MultiRPC.UI;
using MultiRPC.UI.Pages;
using TinyUpdate.Core;
using TinyUpdate.Core.Logging;
using TinyUpdate.Core.Logging.Loggers;

[assembly: SemanticVersion("7.0.0-testing")]
namespace MultiRPC
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            // Windows apps have restricted access, use the appdata folder instead of document.
            Constants.SettingsFolder = Constants.IsWindowsApp ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages/29025FluxpointDevelopment.MultiRPC_q026kjacpk46y/AppData") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MultiRPC-Beta");

            // Set the current directory to the app install location to get assets.
            if (Constants.IsWindowsApp)
                Directory.SetCurrentDirectory(System.AppContext.BaseDirectory + "/");

            var logFolder = Path.Combine(Constants.SettingsFolder, "Logging");
            Directory.CreateDirectory(logFolder);
            LoggingCreator.GlobalLevel = LogLevel.Trace;

            // This seems to break windows apps even though it can write to log folder.
            if (!Constants.IsWindowsApp)
                LoggingCreator.AddLogBuilder(new FileLoggerBuilder(logFolder));

            LoggingCreator.AddLogBuilder(new LoggingPageBuilder());
            var builder = BuildAvaloniaApp();
            builder.StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
