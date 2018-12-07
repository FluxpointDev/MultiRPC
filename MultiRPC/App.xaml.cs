using MultiRPC.GUI;
using System.Windows;
using System.Windows.Threading;

namespace MultiRPC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string SupportServer = "https://discord.gg/";
        public static string Developer = "xXBuilderBXx#8265";

        /// <summary> Main window </summary>
        public static MainWindow WD = null;
        public static bool StartUpdate = false;
        public static string Version = "0.0.0";
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            WD = new MainWindow();
            WD.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorWindow ErrorWindow = new ErrorWindow();
            ErrorWindow.SetError(e);
            ErrorWindow.ShowDialog();
            Current.Shutdown();
        }
    }
}
