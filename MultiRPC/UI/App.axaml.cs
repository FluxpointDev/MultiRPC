using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Pages;
using MultiRPC.UI.Pages.Rpc;
using MultiRPC.UI.Pages.Rpc.Custom;
using Splat;
using TinyUpdate.Binary;
using TinyUpdate.Core.Extensions;
using TinyUpdate.Core.Update;
using TinyUpdate.Github;
using SettingsPage = MultiRPC.UI.Pages.Settings.SettingsPage;

namespace MultiRPC.UI
{
    public class App : Application
    {
        //TODO: Put this somewhere else, for now this works
        private UpdateClient? _updater;
        
        public override void Initialize()
        {
#if !WINSTORE && !DEBUG
            _updater = new GithubUpdateClient(new BinaryApplier(), "FluxpointDev", "MultiRPC");
#endif
            AvaloniaXamlLoader.Load(this);

            //Add default settings here
            Locator.CurrentMutable.Register<Setting.Setting>(() => SettingManager<GeneralSettings>.Setting);
            Locator.CurrentMutable.Register<Setting.Setting>(() => SettingManager<DisableSettings>.Setting);

            //TODO: Replace with splat
            //Any new pages get added here
            PageManager.AddPage(new MultiRpcPage());
            PageManager.AddPage(new CustomPage());
            PageManager.AddPage(new SettingsPage());
            PageManager.AddPage(new LoggingPage());
            PageManager.AddPage(new CreditsPage());
            PageManager.AddPage(new ThemeEditorPage());
            PageManager.AddPage(new MissingPage());

            //Anything else here
            Locator.CurrentMutable.Register(() => new RpcClient());

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
