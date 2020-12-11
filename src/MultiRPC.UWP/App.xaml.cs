using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Common.AssetProcessor;
using MultiRPC.Common.RPC;
using MultiRPC.Core;
using MultiRPC.Core.Page;
using MultiRPC.Core.Rpc;
using MultiRPC.Shared.UI;
using MultiRPC.Shared.UI.Pages;
using MultiRPC.Shared.UI.Pages.Custom;
//using MultiRPC.Shared.UI.Pages.Debug;
using MultiRPC.UWP.AssetProcessor;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MultiRPC.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            //Application.Current.DebugSettings
            //  .IsTextPerformanceVisualizationEnabled = true;

            //These pages have to be added first as they have multiple implementations for different things
            //but we need to only have one instance of them pages
            ServiceManager.Service.AddSingleton<MultiRPCPage>();
            ServiceManager.Service.AddSingleton<CustomPage>();

            //Add their IRpcPage imp first
            ServiceManager.Service.AddSingleton<IRpcPage>(x => x.GetRequiredService<MultiRPCPage>());
            ServiceManager.Service.AddSingleton<IRpcPage>(x => x.GetRequiredService<CustomPage>());

            //Now add their SidePage imp with the other pages
            ServiceManager.Service.AddSingleton<ISidePage>(x => x.GetRequiredService<MultiRPCPage>());
            ServiceManager.Service.AddSingleton<ISidePage>(x => x.GetRequiredService<CustomPage>());
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

            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e?.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e?.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    RpcPageManager.Load();
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        public void GoBack(int count)
        {
            if (Window.Current.Content is MainPage mainPage)
            {
                mainPage.GoBack(count);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
