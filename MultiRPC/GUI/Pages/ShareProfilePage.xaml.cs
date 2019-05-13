using System.Windows;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using System.Windows.Controls;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for ProfileSharePage.xaml
    /// </summary>
    public partial class ShareProfilePage : Page
    {
        private CustomProfile profile;
        private long windowID;

        public ShareProfilePage(CustomProfile _profile, long _windowID)
        {
            InitializeComponent();
            btnExport.Content = App.Text.Export;
            btnImport.Content = App.Text.Import;
            profile = _profile;
            windowID = _windowID;
            Title = App.Text.ProfileShare;
            if (RPC.RPCClient != null && RPC.RPCClient.IsInitialized && !RPC.RPCClient.Disposed)
                btnImport.IsEnabled = false;
        }

        private void ButImport_OnClick(object sender, RoutedEventArgs e)
        {
            string profileBase64 = Newtonsoft.Json.JsonConvert.SerializeObject(tbShare.Text);
            Clipboard.SetText(profileBase64 = Utils.Base64Encode(profileBase64));
            MainWindow.CloseWindow(windowID, profileBase64);
        }

        private async void ButExport_OnClick(object sender, RoutedEventArgs e)
        {
            string profileBase64 = Newtonsoft.Json.JsonConvert.SerializeObject(profile);
            Clipboard.SetText(profileBase64 = Utils.Base64Encode(profileBase64));
            tbShare.Text = profileBase64;
            await CustomMessageBox.Show(App.Text.ProfileCopyMessage);
        }
    }
}
