using MultiRPC.GUI;
using System;
using System.Windows;
using System.Windows.Threading;

namespace MultiRPC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string SupportServer = "https://discord.gg/susQ6XA";
        public static string Developer = "Builderb#0001";
        public static bool SettingsLoaded = false;
        /// <summary> Main window </summary>
        public static MainWindow WD = null;
        public static bool StartUpdate = false;
        public static string Version = "0.0.0";
        public App()
        {
            InitializeComponent();
            try
            {
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                WD = new MainWindow(Resources["ComboBoxStyle"] as Style);
                WD.Show();
            }
            catch(Exception ex)
            {
                ErrorWindow error = new ErrorWindow();
                error.SetUpdateError(ex);
                error.ShowDialog();
            }
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
