using MultiRPC.Functions;
using MultiRPC.GUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public static string Changelog = "";
        public static string Donation = $"Want to support me and my projects?\n\n" +
            $"Consider donating money to help fund my services and keep the projects alive by using Patreon or Paypal\n\n" +
            $"You will gain perks on all my bots and discord server!";
        public App()
        {
            InitializeComponent();
            try
            {
                try
                {
                    Process[] Proc = Process.GetProcessesByName("MultiRPC");
                    if (Proc.Length == 2)
                    {
                        if (File.Exists(RPC.ConfigFolder + "Open.rpc"))
                            File.Delete(RPC.ConfigFolder + "Open.rpc");
                        File.Create(RPC.ConfigFolder + "Open.rpc").Close();
                        File.Delete(RPC.ConfigFolder + "Open.rpc");
                        Current.Shutdown();
                    }
                }
                catch { }
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
