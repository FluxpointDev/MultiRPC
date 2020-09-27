using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using MultiRPC.Shared.UI.Views;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using static MultiRPC.Core.LanguagePicker;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopBar : LocalizablePage
    {
        private IRpcClient RpcClient;

        //TODO: Move this to RPC client
        ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

        public TopBar()
        {
            InitializeComponent();
            RpcClient = ServiceManager.ServiceProvider.GetService<IRpcClient>();
            RpcPageManager.NewCurrentPage += RpcPageManager_NewCurrentPage;            
        }

        private void RpcPageManager_NewCurrentPage(object sender, IRpcPage e)
        {
            btnStart.Content = StartText;
        }

        private string StartText =>
            $"{GetLineFromLanguageFile(RpcClient.IsRunning ? "Stop" : "Start")} {GetLineFromLanguageFile(RpcPageManager.CurrentPage?.LocalizableName)}";

        public override void UpdateText()
        {
            btnStart.Content = StartText;
            btnUpdatePresence.Content = GetLineFromLanguageFile("UpdatePresence");

            rStatus.Text = GetLineFromLanguageFile("Status") + ": ";
            rConnectStatus.Text = GetLineFromLanguageFile(ConnectionStatus.ToString());
            rUser.Text = GetLineFromLanguageFile("User") + ": ";
            rUsername.Text = "Azy#0000"; //TODO: Store and get username
            tblAfk.Text = GetLineFromLanguageFile("AfkText") + ": ";

            btnAuto.Content = GetLineFromLanguageFile("Auto");
            btnAfk.Content = GetLineFromLanguageFile("Afk");
        }

        public void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (RpcClient.IsRunning)
            {
                RpcClient.Start(RpcPageManager.CurrentPage.RichPresence.ApplicationId);
            }
            else
            {
                RpcClient.Stop();
            }
        }
    }

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
    }
}
