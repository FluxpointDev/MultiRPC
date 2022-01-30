using Splat;
using Avalonia;
using MultiRPC.Rpc;
using MultiRPC.Utils;
using System.Net.Http;
using MultiRPC.Setting;
using MultiRPC.Theming;
using MultiRPC.UI.Pages;
using Avalonia.Markup.Xaml;
using MultiRPC.UI.Pages.Rpc;
using TinyUpdate.Core.Update;
using MultiRPC.UI.Pages.Theme;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Pages.Settings;
using Avalonia.Controls.ApplicationLifetimes;
using TinyUpdate.Binary;
using TinyUpdate.Github;
using TinyUpdate.Core.Extensions;
using System.Runtime.InteropServices;
using Avalonia.Themes.Fluent;
using MultiRPC.Updating;

namespace MultiRPC.UI;

/*TODO:
 * Styling:
   * CheckBox (Actual Box)
   * Tooltip
   * Text on purple RpcView
   * Min/Max Buttons*/
public class App : Application
{
    public static readonly HttpClient HttpClient = new HttpClient();

    //TODO: Put this somewhere else, for now this works
    private UpdateClient? _updater;

    public override void Initialize()
    {
#if !DEBUG
#if _UWP
        _updater = new WinStoreUpdater();
#else
        _updater = new GithubUpdateClient(new BinaryApplier(), "FluxpointDev", "MultiRPC");
#endif
#endif
        AvaloniaXamlLoader.Load(this);
        var genSettings = SettingManager<GeneralSettings>.Setting;
        var theme = (genSettings.ThemeFile?.StartsWith('#') ?? false)
            ? Themes.ThemeIndexes[genSettings.ThemeFile] 
            : (Theme.Load(genSettings.ThemeFile) ?? Themes.Dark);

        Theme.ActiveThemeChanged += (sender, newTheme) =>
        {
            ((FluentTheme)Styles[0]).Mode = (FluentThemeMode)newTheme.Metadata.Mode;
        };
        theme.Apply();

        //Add default settings here
        Locator.CurrentMutable.RegisterLazySingleton<BaseSetting>(() => genSettings);
        Locator.CurrentMutable.RegisterLazySingleton<BaseSetting>(() => SettingManager<DisableSettings>.Setting);

        //Any new pages get added here
        PageManager.AddPage(new MultiRpcPage());
        PageManager.AddPage(new CustomPage());
        PageManager.AddPage(new SettingsPage());
        PageManager.AddPage(new LoggingPage());
        PageManager.AddPage(new CreditsPage());
        PageManager.AddPage(new MasterThemeEditorPage());
        if (DebugUtil.IsDebugBuild)
        {
            PageManager.AddPage(new DebugPage());
        }

        //Anything else here
        Locator.CurrentMutable.RegisterLazySingleton(() => new RpcClient());

#if !DEBUG
        /*if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _= _updater?.UpdateApp(null);
        }*/
#endif
    }

    public IClassicDesktopStyleApplicationLifetime? DesktopLifetime;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DesktopLifetime = desktop;
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}