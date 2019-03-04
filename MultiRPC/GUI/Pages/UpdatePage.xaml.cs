using MultiRPC.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page
    {
        public UpdatePage()
        {
            InitializeComponent();
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://multirpc.blazedev.me/Changelog.txt", App.ConfigFolder + "Changelog.txt");
                }
                using (StreamReader reader = new StreamReader(App.ConfigFolder + "Changelog.txt"))
                {
                    App.Changelog = reader.ReadToEnd();
                }
                Changelog.Text = App.Changelog;
            }
            catch (Exception ex)
            {
                Changelog.Text = $"Error getting changelog text, {ex.Message}";
            }
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            App.BW.LoadMain();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            BtnSkip.Visibility = Visibility.Hidden;
            BtnUpdate.Visibility = Visibility.Hidden;
            FuncUpdater.Start();
        }
    }
}
