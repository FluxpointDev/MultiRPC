using Microsoft.Extensions.Logging;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MultiRPC.Core;
using MultiRPC.Shared.UI.Pages;
using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Wasm.AssetProcessor;
using MultiRPC.Core.Rpc;
using MultiRPC.Shared.UI;
using MultiRPC.Common.AssetProcessor;
using MultiRPC.Common.RPC;

namespace MultiRPC.Wasm
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
            ConfigureFilters(Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);

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
            RpcPageManager.Load();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
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


        /// <summary>
        /// Configures global logging
        /// </summary>
        /// <param name="factory"></param>
        static void ConfigureFilters(ILoggerFactory factory)
        {
	        factory
		        .WithFilter(new FilterLoggerSettings
			        {
				        {"Uno", LogLevel.Debug},
				        {"Windows", LogLevel.Warning},

				        // Debug JS interop
				        { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

				        // Generic Xaml events
				        { "Windows.UI.Xaml", LogLevel.Debug },
				        { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
				        { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
				        { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

				        // Layouter specific messages
				        // { "Windows.UI.Xaml.Controls", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
				        // { "Windows.Storage", LogLevel.Debug },

				        // Binding related messages
				        // { "Windows.UI.Xaml.Data", LogLevel.Debug },

				        // DependencyObject memory references tracking
				        // { "ReferenceHolder", LogLevel.Debug },

				        // ListView-related messages
				        // { "Windows.UI.Xaml.Controls.ListViewBase", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.ListView", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.GridView", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.VirtualizingPanelLayout", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.NativeListViewBase", LogLevel.Debug },
				        // { "Windows.UI.Xaml.Controls.ListViewBaseSource", LogLevel.Debug }, //iOS
				        // { "Windows.UI.Xaml.Controls.ListViewBaseInternalContainer", LogLevel.Debug }, //iOS
				        // { "Windows.UI.Xaml.Controls.NativeListViewBaseAdapter", LogLevel.Debug }, //Android
				        // { "Windows.UI.Xaml.Controls.BufferViewCache", LogLevel.Debug }, //Android
				        // { "Windows.UI.Xaml.Controls.VirtualizingPanelGenerator", LogLevel.Debug }, //WASM
			        }
		        );
#if DEBUG
	        //.AddConsole(LogLevel.Debug);
#else
            //.AddConsole(LogLevel.Information);
#endif
        }
    }
}
