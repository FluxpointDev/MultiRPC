using Splat;
using Avalonia;
using MultiRPC.Rpc;
using MultiRPC.Utils;
using MultiRPC.Setting;
using MultiRPC.Theming;
using MultiRPC.UI.Pages;
using Avalonia.Markup.Xaml;
using MultiRPC.UI.Pages.Rpc;
using TinyUpdate.Core.Update;
using MultiRPC.UI.Pages.Theme;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Pages.Settings;
using MultiRPC.Updating;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using TinyUpdate.Binary;
using TinyUpdate.Github;

namespace MultiRPC.UI;

/*TODO:
 * Styling:
   * CheckBox (Actual Box)
   * Tooltip
   * Min/Max Buttons*/
public class App : Application
{
    public static readonly HttpClient HttpClient = new HttpClient();
    public static MainWindow MainWindow;

    //TODO: Put this somewhere else, for now this works
    private UpdateClient? _updater;
    private Action _closeAction;

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
        var theme = (genSettings.ThemeFile != null && genSettings.ThemeFile.StartsWith('#') && Themes.ThemeIndexes.ContainsKey(genSettings.ThemeFile))
            ? Themes.ThemeIndexes[genSettings.ThemeFile] 
            : (Theme.Load(genSettings.ThemeFile) ?? Themes.Default);
        
        Theme.ActiveThemeChanged += (sender, newTheme) =>
        {
            ((FluentTheme)Styles[0]).Mode = (FluentThemeMode)newTheme.Metadata.Mode;
        };
        theme.Apply();

        //Add settings here
        Locator.CurrentMutable.RegisterLazySingleton(() => genSettings, typeof(IBaseSetting));
        Locator.CurrentMutable.RegisterLazySingleton(() => SettingManager<DisableSettings>.Setting, typeof(IBaseSetting));

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

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _closeAction = () => desktop.Shutdown();
            desktop.MainWindow = MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Exit() => _closeAction.Invoke();
}