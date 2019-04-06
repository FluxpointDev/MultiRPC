using System.Windows;
using System.Windows.Controls;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for ProfileSharePage.xaml
    /// </summary>
    public partial class ShareProfilePage : Page
    {
        private CustomProfile Profile;
        private long WindowID;

        public ShareProfilePage(CustomProfile profile, long windowID)
        {
            InitializeComponent();
            btnExport.Content = App.Text.Export;
            btnImport.Content = App.Text.Import;
            Profile = profile;
            WindowID = windowID;
            Title = App.Text.ProfileShare;
            if (RPC.RPCClient != null && RPC.RPCClient.IsInitialized && !RPC.RPCClient.Disposed)
                btnImport.IsEnabled = false;
        }

        private void ButImport_OnClick(object sender, RoutedEventArgs e)
        {
            string Get = Newtonsoft.Json.JsonConvert.SerializeObject(tbShare.Text);
            Clipboard.SetText(Get = Utils.Base64Encode(Get));
            MainWindow.CloseWindow(WindowID, Get);
        }

        private async void ButExport_OnClick(object sender, RoutedEventArgs e)
        {
            string Get = Newtonsoft.Json.JsonConvert.SerializeObject(Profile);
            Clipboard.SetText(Get = Utils.Base64Encode(Get));
            tbShare.Text = Get;
            await CustomMessageBox.Show(App.Text.ProfileCopyMessage);
        }
    }
}
