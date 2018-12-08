using System.Deployment.Application;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window 
    {
        public bool Test = false;
        public UpdateCheckInfo Info;
        public UpdateWindow()
        {
            InitializeComponent();
            Loaded += UpdateWindow_Loaded;
        }

        private void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TitleOldVersion.Content = $"Current Version: {App.Version}";
            if (Test)
            {
                Title = "TEST - Update";
                Changelog.Text = "Hello this is a nice changelog";
                Height += 10;
            }
            else
            {
                if (File.Exists(RPC.ConfigFolder + "Changelog.txt"))
                {
                    using (StreamReader reader = new StreamReader(RPC.ConfigFolder + "Changelog.txt"))
                    {
                        Changelog.Text = reader.ReadToEnd();
                    }
                }
                TitleNewVersion.Content = $"New Version: {Info.AvailableVersion.Major}.{Info.AvailableVersion.Minor}.{Info.AvailableVersion.Build}";
                
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (Test)
                MessageBox.Show("Honk honk");
            else
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply reply = ping.Send("multirpc.blazedev.me");
                    App.StartUpdate = true;
                }
                catch
                {
                    RPC.Log.Error("App", "Could not ping multirpc.blazedev.me");
                    MessageBox.Show("Could not contact download server");
                }
                Close();
            }
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
