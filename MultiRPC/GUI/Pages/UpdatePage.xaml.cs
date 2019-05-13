using System.IO;
using System.Windows;
using System.Reflection;
using MultiRPC.Functions;
using System.Diagnostics;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Deployment.Application;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page
    {
        private long windowID;

        public UpdatePage(UpdateCheckInfo info, long _windowID)
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

            if(File.Exists(FileLocations.ChangelogFileLocalLocation))
                tbChangelogText.Text = File.ReadAllText(FileLocations.ChangelogFileLocalLocation);

            windowID = _windowID;
            Title = App.Text.Update;
        }

        private async void ButSkip_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(windowID, false);
            await MainPage._MainPage.Dispatcher.Invoke(async () =>
            {
                MainPage._MainPage.pbUpdateProgress.Visibility = Visibility.Collapsed;
            });
        }

        private async void ButUpdateNow_OnClick(object sender, RoutedEventArgs e)
        {
            if (!App.Config.AutoUpdate)
            {
                MainWindow.CloseWindow(windowID, true);
                await Task.Delay(250);
                Updater.Start();
            }
            else
            {
                if (File.Exists(FileLocations.MultiRPCStartLink))
                    Process.Start(FileLocations.MultiRPCStartLink);
                App.Current.Shutdown();
            }
        }
    }
}
