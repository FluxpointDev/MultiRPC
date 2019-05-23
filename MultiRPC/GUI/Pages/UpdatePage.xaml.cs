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
    ///     Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page
    {
        private readonly long _windowID;

        public UpdatePage(UpdateCheckInfo info, long windowID)
        {
            InitializeComponent();
            if (App.Config.AutoUpdate)
            {
                btnSkip.Content = App.Text.DontRestartProgram;
                btnUpdateNow.Content = App.Text.RestartProgram;
            }
            else
            {
                btnSkip.Content = App.Text.Skip;
                btnUpdateNow.Content = App.Text.UpdateNow;
            }

            tblCurrentVersion.Text =
                App.Text.CurrentVersion + ": " + Assembly.GetExecutingAssembly().GetName().Version;
            if (info != null)
                tblNewVersion.Text = App.Text.NewVersion + ": " + info.AvailableVersion;
            else
                tblNewVersion.Text = App.Text.NewVersion + ": " + "???";

            if (File.Exists(FileLocations.ChangelogFileLocalLocation))
                tbChangelogText.Text = File.ReadAllText(FileLocations.ChangelogFileLocalLocation);

            _windowID = windowID;
            Title = App.Text.Update;
        }

        private void ButSkip_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(_windowID, false);
            MainPage._MainPage.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
            });
        }

        private async void ButUpdateNow_OnClick(object sender, RoutedEventArgs e)
        {
            if (!App.Config.AutoUpdate)
            {
                MainWindow.CloseWindow(_windowID, true);
                await Task.Delay(250);
                Updater.Start();
            }
            else
            {
                if (File.Exists(FileLocations.MultiRPCStartLink))
                    Process.Start(FileLocations.MultiRPCStartLink, "--fromupdate");
                Application.Current.Shutdown();
            }
        }
    }
}