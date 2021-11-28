using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using MultiRPC.Extensions;
using MultiRPC.Logging;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI;
using MultiRPC.Utils;
using TinyUpdate.Core;
using TinyUpdate.Core.Logging;
using TinyUpdate.Core.Logging.Loggers;

//TODO: Make GIFs loops stop when they're not on screen
//TODO: Make this be used for the versioning
[assembly: SemanticVersion("7.0.0-beta4")]
namespace MultiRPC
{
    class Program
    {
        private static IPC _ipc;
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            LoggingCreator.GlobalLevel = SettingManager<GeneralSettings>.Setting.LogLevel;

            // Set the current directory to the app install location to get assets.
#if WINSTORE
            Directory.SetCurrentDirectory(AppContext.BaseDirectory + "/");
#else
            // This seems to break windows apps even though it *can* write to the log folder.
            var logFolder = Path.Combine(Constants.SettingsFolder, "Logging");
            Directory.CreateDirectory(logFolder);
            LoggingCreator.AddLogBuilder(new FileLoggerBuilder(logFolder));
#endif
            LoggingCreator.AddLogBuilder(new LoggingPageBuilder());
            _ipc = IPC.GetOrMakeConnection();
            
            //Now check if we are already currently open, if so tell that instance to show
            if (!DebugUtil.IsDebugBuild && Process.GetProcessesByName("MultiRPC").Length > 1)
            {
                var sent = _ipc.SendMessage("SHOW");
                _ipc.DisconnectFromServer();
                Environment.Exit(sent ? 0 : -1);
            }
            _ipc.NewMessage += IpcOnNewMessage;

            var builder = BuildAvaloniaApp();
            builder.StartWithClassicDesktopLifetime(args);
            _ipc.StopServer();
        }

        //For now this just handles showing the Window again but can be used for other things later on
        private static void IpcOnNewMessage(object? sender, string e)
        {
            if (e == "SHOW")
            {
                var win = ((App)Application.Current).DesktopLifetime!.MainWindow;
                win.RunUILogic(() => win.WindowState = WindowState.Normal);
            }
        }

        //TODO: Make logger for this
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions
                {
                    EnableMultitouch = true
                })
                .LogToTrace();
    }
}