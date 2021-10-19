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
            var logFolder = Path.Combine(Constants.SettingsFolder, "Logging");
            Directory.CreateDirectory(logFolder);

            LoggingCreator.GlobalLevel = LogLevel.Trace;
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
