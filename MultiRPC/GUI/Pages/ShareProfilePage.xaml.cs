using System.Windows;
using System.Windows.Controls;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using Newtonsoft.Json;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for ProfileSharePage.xaml
    /// </summary>
    public partial class ShareProfilePage : Page
    {
        private readonly CustomProfile _profile;
        private readonly long _windowID;

        public ShareProfilePage(CustomProfile profile, long windowID)
        {
            InitializeComponent();
            btnExport.Content = App.Text.Export;
            btnImport.Content = App.Text.Import;
            _profile = profile;
            _windowID = windowID;
            Title = App.Text.ProfileShare;
            if (RPC.RPCClient != null && RPC.RPCClient.IsInitialized && !RPC.RPCClient.Disposed)
                btnImport.IsEnabled = false;
        }

        private void ButImport_OnClick(object sender, RoutedEventArgs e)
        {
            var profileBase64 = JsonConvert.SerializeObject(tbShare.Text);
            Clipboard.SetText(profileBase64 = Utils.Base64Encode(profileBase64));
            MainWindow.CloseWindow(_windowID, profileBase64);
        }

        private async void ButExport_OnClick(object sender, RoutedEventArgs e)
        {
            var profileBase64 = JsonConvert.SerializeObject(_profile);
            Clipboard.SetText(profileBase64 = Utils.Base64Encode(profileBase64));
            tbShare.Text = profileBase64;
            await CustomMessageBox.Show(App.Text.ProfileCopyMessage, window: MainWindow.GetWindow(_windowID));
        }
    }
}