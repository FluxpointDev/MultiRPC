using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page
    {
        private long WindowID;

        public UpdatePage(UpdateCheckInfo info, long windowID)
        {
            InitializeComponent();
            if (App.Config.AutoUpdate)
            {
                butSkip.Content = App.Text.DontRestartProgram;
                butUpdateNow.Content = App.Text.RestartProgram;
            }
            else
            {
                butSkip.Content = App.Text.Skip;
                butUpdateNow.Content = App.Text.UpdateNow;
            }

            tblCurrentVersion.Text =
                App.Text.CurrentVersion + ": " + Assembly.GetExecutingAssembly().GetName().Version;
            if (info != null)
                tblNewVersion.Text = App.Text.NewVersion + ": " + info.AvailableVersion;
            else
                tblNewVersion.Text = App.Text.NewVersion + ": " + "???";
            tbChangelogText.Text = File.ReadAllText(FileLocations.ChangelogFileLocalLocation);

            WindowID = windowID;
        }

        private async void ButSkip_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(WindowID, false);
            await MainPage.mainPage.Dispatcher.Invoke(async () =>
            {
                MainPage.mainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
            });
        }

        private async void ButUpdateNow_OnClick(object sender, RoutedEventArgs e)
        {
            if (!App.Config.AutoUpdate)
            {
                MainWindow.CloseWindow(WindowID, true);
                await Task.Delay(250);
                Updater.Start();
            }
            else
            {
                string filepath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/MultiRPC.appref-ms";
                if (File.Exists(filepath))
                    Process.Start(filepath);
                App.Current.Shutdown();
            }
        }
    }
}
