using MultiRPC.Functions;
using MultiRPC.GUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MultiRPC.GUI.Pages;

namespace MultiRPC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string SupportServer = "https://discord.gg/susQ6XA";
        public static string Developer = "Builderb#0001";
        public static bool Crashed = false;
        public static bool FormReady = false;
        public static Config Config = new Config();
        public static string ConfigFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC/";
        public static string ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC/Config.json";
        public static string ProfilesFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC/Profiles.json";
        public static Logger Log = new Logger();
        public static MainPage WD = null;
        public static BaseWindow BW = null;
        public static bool StartUpdate = false;
        public static string Version = "4.2.4";
        public static string Changelog = "";
        public static string Donation =
            $"Consider donating money to help fund my services to keep the projects alive by using Patreon or Paypal\n\n" +
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
                        if (File.Exists(ConfigFolder + "Open.rpc"))
                            File.Delete(ConfigFolder + "Open.rpc");
                        File.Create(ConfigFolder + "Open.rpc").Close();
                        File.Delete(ConfigFolder + "Open.rpc");
                        Current.Shutdown();
                    }
                }
                catch { }
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                BW = new BaseWindow();
                WD = new MainPage(Resources["ComboBoxStyle"] as Style, BW);
                var frame = (Frame)BW.FindName("MainFrame");
                frame.Content = WD;
                BW.Show();
            }
            catch(Exception ex)
            {
                Crashed = true;
                ErrorWindow error = new ErrorWindow();
                error.SetUpdateError(ex);
                error.ShowDialog();
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Crashed = true;
            ErrorWindow ErrorWindow = new ErrorWindow();
            ErrorWindow.SetError(e);
            ErrorWindow.ShowDialog();
            Current.Shutdown();
        }
    }
}
