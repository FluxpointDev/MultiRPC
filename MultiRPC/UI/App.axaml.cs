using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;
using MultiRPC.UI.Pages;
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
        private UpdateClient Updater = new GithubUpdateClient(new BinaryApplier(),"FluxpointDev", "MultiRPC");
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            //TODO: Add all pages
            PageManager.AddRpcPage(new MultiRpcPage());
            PageManager.AddRpcPage(new CustomPage());
            PageManager.AddPage(new MissingPage());
            RpcPageManager.GiveRpcClient(RpcClient);

            _= Updater.UpdateApp(null);
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
