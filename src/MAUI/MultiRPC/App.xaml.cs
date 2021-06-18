using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using MultiRPC.Shared.UI;
using MultiRPC.Shared.UI.Pages;
using MultiRPC.Shared.UI.Pages.Custom;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiRPC
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

            ServiceManager.AddSingleton<MainPage>();
            ServiceManager.Service.AddDefaults();


            //Now to process everything ready for the Client to use them
            ServiceManager.ProcessService();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow(ServiceManager.ServiceProvider.GetRequiredService<MainPage>());
            m_window.Activate();
        }

        private Window m_window;
    }
}
