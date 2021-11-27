using System.Net.Http;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Pages;
using MultiRPC.UI.Pages.Rpc;
using MultiRPC.UI.Pages.Rpc.Custom;
using MultiRPC.UI.Pages.Settings;
using Splat;
using TinyUpdate.Binary;
using TinyUpdate.Core.Extensions;
using TinyUpdate.Core.Update;
using TinyUpdate.Github;

namespace MultiRPC.UI
{
    public class App : Application
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        //TODO: Put this somewhere else, for now this works
        private UpdateClient? _updater;
        
        public override void Initialize()
        {
#if !WINSTORE && !DEBUG
            _updater = new GithubUpdateClient(new BinaryApplier(), "FluxpointDev", "MultiRPC");
#endif
            AvaloniaXamlLoader.Load(this);

            //Add default settings here
            Locator.CurrentMutable.RegisterLazySingleton<BaseSetting>(() => SettingManager<GeneralSettings>.Setting);
            Locator.CurrentMutable.RegisterLazySingleton<BaseSetting>(() => SettingManager<DisableSettings>.Setting);

            //Any new pages get added here
            PageManager.AddPage(new MultiRpcPage());
            PageManager.AddPage(new CustomPage());
            PageManager.AddPage(new SettingsPage());
            PageManager.AddPage(new LoggingPage());
            PageManager.AddPage(new CreditsPage());
            PageManager.AddPage(new ThemeEditorPage());

            //Anything else here
            Locator.CurrentMutable.RegisterLazySingleton(() => new RpcClient());

#if !DEBUG
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _= _updater?.UpdateApp(null);
            }
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
}
