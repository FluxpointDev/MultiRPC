using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using Microsoft.Extensions.DependencyInjection;
using static MultiRPC.Core.LanguagePicker;
using System.Threading.Tasks;
using System;
using Microsoft.UI.Xaml.Controls;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopBar : LocalizablePage
    {
        private RichPresence AfkPresence = new RichPresence("Afk", Constants.AfkID) 
        {
            Assets = new Assets
            { 
                LargeImage = new Core.Rpc.Image
                {
                    Key = "cat"
                }
            }
        };

        private IRpcClient RpcClient;

        public TopBar()
        {
            InitializeComponent();
            RpcClient = ServiceManager.ServiceProvider.GetService<IRpcClient>();

            RpcClient.Disconnected += RpcClient_Disconnected;
            RpcClient.Ready += RpcClient_Ready;
            RpcClient.Loading += RpcClient_Loading;
            RpcClient.Errored += RpcClient_Errored;
            RpcClient.PresenceUpdated += RpcClient_PresenceUpdated;

            RpcPageManager.NewCurrentPage += RpcPageManager_NewCurrentPage;
        }

        //To update the RPC View's presence
        private void RpcClient_PresenceUpdated(object sender, RichPresence e) => 
            rpcView.RichPresence = e;

        private void RpcClient_Errored(object sender, EventArgs e) =>
            rpcView.CurrentView = Views.RPCView.ViewType.Error;

        private void RpcClient_Loading(object sender, EventArgs e)
        {
            rpcView.CurrentView = Views.RPCView.ViewType.Loading;
            UpdateButtons();
            UpdateText();
        }

        private void RpcClient_Ready(object sender, EventArgs e)
        {
            rpcView.CurrentView = Views.RPCView.ViewType.RichPresence;
            UpdateText();
        }

        private void RpcClient_Disconnected(object sender, bool e)
        {
            rpcView.CurrentView = Views.RPCView.ViewType.Default;
            UpdateButtons();
            UpdateText();
        }

        //Updates the Start button when we go to another RPC Page
        private async void RpcPageManager_NewCurrentPage(object sender, IRpcPage e)
        {
            btnStart.Content = await StartText();
        }

        private async Task<string> StartText() 
        {
            if (RpcClient.IsRunning)
            {
                return await GetLineFromLanguageFile("Shutdown");
            }
            return $"{await GetLineFromLanguageFile("Start")} {await GetLineFromLanguageFile(RpcPageManager.CurrentPage?.LocalizableName)}";
        }

        public override async void UpdateText() => 
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
        {
            btnStart.Content = await StartText();
            btnUpdatePresence.Content = await GetLineFromLanguageFile("UpdatePresence");

            rStatus.Text = await GetLineFromLanguageFile("Status") + ": ";
            rConnectStatus.Text = await GetLineFromLanguageFile(
                RpcClient.Status == ConnectionStatus.Connecting ? "Loading" : RpcClient.Status.ToString());
            rUser.Text = await GetLineFromLanguageFile("User") + ": ";
            rUsername.Text = "Azy#0000"; //TODO: Store and get username
            tblAfk.Text = await GetLineFromLanguageFile("AfkText") + ": ";

            btnAuto.Content = await GetLineFromLanguageFile("Auto");
            btnAfk.Content = await GetLineFromLanguageFile("Afk");
        }).AsTask();

        public async void btnUpdatePresence_Click(object sender, RoutedEventArgs e) => 
            RpcClient?.UpdatePresence(RpcPageManager.CurrentPage.RichPresence);

        public async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!RpcClient.IsRunning)
            {
                RpcClient.Start(RpcPageManager.CurrentPage.RichPresence.ApplicationId);
            }
            else
            {
                RpcClient.Stop();
            }
        }

        public async void btnAfk_Click(object sender, RoutedEventArgs e)
        {
            if (RpcClient.IsRunning && RpcClient.ActivePresence.ApplicationId != AfkPresence.ApplicationId)
            {
                RpcClient.Stop();
            }
            AfkPresence.Details = txtAfk.Text;
            RpcClient.UpdatePresence(AfkPresence);

            if (!RpcClient.IsRunning)
            {
                RpcClient.Start(AfkPresence.ApplicationId);
            }
        }

        private void txtAfk_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnAfk.IsEnabled = txtAfk.Text.Length != 1;
        }

        private async void UpdateButtons()
        {
            btnUpdatePresence.Style = RpcClient.IsRunning ? (Style)Application.Current.Resources["btnPurple"] : null;
            btnStart.Style = (Style)Application.Current.Resources[RpcClient.IsRunning ? "btnRed" : "btnGreen"];
            btnUpdatePresence.IsEnabled = RpcClient.IsRunning;
            btnStart.Content = await StartText();
        }
    }
}
