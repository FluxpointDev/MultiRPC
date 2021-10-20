using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.UI.Pages;
using MultiRPC.UI.Pages.Rpc;
using TinyUpdate.Binary;
using TinyUpdate.Core.Extensions;
using TinyUpdate.Core.Update;
using TinyUpdate.Github;

namespace MultiRPC.UI
{
    public class App : Application
    {
        public static readonly RpcClient RpcClient = new RpcClient();

        //TODO: Put this somewhere else, for now this works
        private UpdateClient Updater = null;
        
        public override void Initialize()
        {
            if (!Constants.IsWindowsApp)
                Updater = new GithubUpdateClient(new BinaryApplier(), "FluxpointDev", "MultiRPC");
            AvaloniaXamlLoader.Load(this);

            PageManager.AddRpcPage(new MultiRpcPage());
            PageManager.AddRpcPage(new CustomPage());
            PageManager.AddPage(new SettingsPage());
            PageManager.AddPage(new LoggingPage());
            PageManager.AddPage(new CreditsPage());
            PageManager.AddPage(new ThemeEditorPage());
            PageManager.AddPage(new MissingPage());
            RpcPageManager.GiveRpcClient(RpcClient);

            if (!Constants.IsWindowsApp && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _= Updater.UpdateApp(null);
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
