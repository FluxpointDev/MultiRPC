using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MultiRPC.Shared.UI.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MultiRPC.Core;
using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core.Rpc;
using MultiRPC.Common.RPC;
using MultiRPC.Common.AssetProcessor;
using MultiRPC.WinUI3.AssetProcessor;
using MultiRPC.Shared.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using MultiRPC.Core.Page;
using MultiRPC.Shared.UI.Pages.Custom;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiRPC.WinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;

            ServiceManager.Service.AddSingleton(x => m_window.MainPage);

            //These pages have to be added first as they have multiple implementations for different things
            //but we need to only have one instance of them pages
            ServiceManager.Service.AddSingleton<MultiRPCPage>();
            ServiceManager.Service.AddSingleton<CustomPage>();
            ServiceManager.Service.AddSingleton<CustomPageContainer>();

            //Add their IRpcPage imp first
            ServiceManager.Service.AddSingleton<IRpcPage>(x => x.GetRequiredService<MultiRPCPage>());
            ServiceManager.Service.AddSingleton<IRpcPage>(x => x.GetRequiredService<CustomPage>());

            //Now add their SidePage imp with the other pages
            ServiceManager.Service.AddSingleton<ISidePage>(x => x.GetRequiredService<MultiRPCPage>());
            ServiceManager.Service.AddSingleton<ISidePage>(x => x.GetRequiredService<CustomPageContainer>());
            ServiceManager.Service.AddSingleton<ISidePage, SettingsPage>();
            ServiceManager.Service.AddSingleton<ISidePage, LoggingPage>();
            ServiceManager.Service.AddSingleton<ISidePage, CreditsPage>();
            ServiceManager.Service.AddSingleton<ISidePage, ThemeEditorPage>();

#if DEBUG
            //Add any debugging pages into here
            //ServiceManager.Service.AddSingleton<ISidePage, RPCViewTestPage>();
#endif
            //Add the FileSystemAccess service because UWP be a pain and make their own and not using System.IO
            //like everyone else 😑
            ServiceManager.Service.AddSingleton<IFileSystemAccess, FileSystemAccess>();

            //Add our asset processors so we can use assets xP
            ServiceManager.Service.AddSingleton<IAssetProcessor, PageIconProcessor>();
            ServiceManager.Service.AddSingleton<IAssetProcessor, LogoProcessor>();
            ServiceManager.Service.AddSingleton<IAssetProcessor, GifProcessor>();

            //Add our RpcClient
            ServiceManager.Service.AddSingleton<IRpcClient, RpcClient>();

            //Now to process everything ready for the Client to use them
            ServiceManager.ProcessService();
            RpcPageManager.Load();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (e.Exception.GetType() == typeof(NotImplementedException))
            {
                e.Handled = true;
                m_window.ShowNotImplementedPage();
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        public void GoBack(int count = 1)
        {
            m_window.GoBack(count);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            // Save application state and stop any background activity
        }

        private MainWindow m_window;
    }
}
