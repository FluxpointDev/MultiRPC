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

[assembly: SemanticVersion("7.0.0-beta6")]
namespace MultiRPC;

internal static class Program
{
    private static IPC _ipc = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        LoggingCreator.GlobalLevel = SettingManager<GeneralSettings>.Setting.LogLevel;

        // Set the current directory to the app install location to get assets.
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
#if !_UWP
        // This seems to break windows apps even though it *can* write to the log folder.
        Directory.CreateDirectory(Constants.LogFolder);
        LoggingCreator.AddLogBuilder(new FileLoggerBuilder(Constants.LogFolder));
#endif
        LoggingCreator.AddLogBuilder(new LoggingPageBuilder());
        _ipc = IPC.GetOrMakeConnection();

        /*Now check if we are already currently open, if so
         tell that instance to show and exit (with code based on if the message was sent)*/
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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions()).LogToTinyUpdate("Property", "Visual", "Layout");
}