using System;
using System.Linq;
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
using Avalonia.Media;

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
        
        // This fixes debugging on visual studio :( 
        if (theme == null)
        {
            theme = new Theme
            {
                _hasAssets = false,
                Colours = new Colours
                {
                    ThemeAccentColor = Color.FromRgb(54, 57, 62),
                    ThemeAccentColor2 = Color.FromRgb(44, 46, 48),
                    ThemeAccentColor2Hover = Color.FromRgb(44, 42, 42),
                    ThemeAccentColor3 = Color.FromRgb(255, 255, 255),
                    ThemeAccentColor4 = Color.FromRgb(180, 180, 180),
                    ThemeAccentColor5 = Color.FromRgb(112, 112, 122),
                    TextColour = Color.FromRgb(255, 255, 255),
                    ThemeAccentDisabledColor = Color.FromRgb(80, 80, 80),
                    ThemeAccentDisabledTextColor = Color.FromRgb(255, 255, 255),
                    NavButtonSelectedColor = Color.FromRgb(0, 171, 235),
                    NavButtonSelectedIconColor = Color.FromRgb(255, 255, 255),
                },
                Metadata = new Metadata("Dark", new SemVersion.SemanticVersion(7, 0, 0, "beta7")),
                Location = "#Dark",
                ThemeType = ThemeType.Modern
            };
        }
        Theme.ActiveThemeChanged += (sender, newTheme) =>
        {
            ((FluentTheme)Styles[0]).Mode = (FluentThemeMode)newTheme.Metadata.Mode;
        };
        theme.Apply();

        //Add settings here
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