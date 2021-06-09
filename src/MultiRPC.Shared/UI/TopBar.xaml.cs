using System;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using MultiRPC.Core.Page;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static MultiRPC.Core.LanguagePicker;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopBar : LocalizablePage
    {
        public MainPage MainPage;

        private readonly RichPresence AfkPresence = new("Afk", Constants.AfkID) 
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
            //To Do: Hook up RpcPageManager to something so we can change btnStart IsEnabled
            InitializeComponent();
            /*RpcClient = ServiceManager.ServiceProvider.GetService<IRpcClient>();

            RpcClient.Disconnected += RpcClient_Disconnected;
            RpcClient.Ready += RpcClient_Ready;
            RpcClient.Loading += RpcClient_Loading;
            RpcClient.Errored += RpcClient_Errored;
            RpcClient.PresenceUpdated += RpcClient_PresenceUpdated;*/

            //RpcPageManager.PageChanged += RpcPageManager_NewCurrentPage;
            //RpcPageManager.PagePropertyChanged += RpcPageManager_PagePropertyChanged;

            Loaded += TopBar_Loaded;
        }

        private void RpcPageManager_PagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //UpdateButtons();
        }

        private void TopBar_Loaded(object sender, RoutedEventArgs e)
        {
            //Got to do it here as the MainPage won't be made until the application has loaded
            //MainPage = ServiceManager.ServiceProvider.GetService<MainPage>();
            //Loaded -= TopBar_Loaded;
        }

        //To update the RPC View's presence
        /*private void RpcClient_PresenceUpdated(object sender, RichPresence e) => 
            rpcView.RichPresence = e;

        private void RpcClient_Errored(object sender, EventArgs e) =>
            rpcView.CurrentView = RPCView.ViewType.Error;

        private async void RpcClient_Loading(object sender, EventArgs e)
        {
            rpcView.CurrentView = RPCView.ViewType.Loading;
            PartialUpdate();
        }

        private void RpcClient_Ready(object sender, EventArgs e)
        {
            rpcView.CurrentView = RPCView.ViewType.RichPresence;
            UpdateText();
        }

        private async void RpcClient_Disconnected(object sender, bool e)
        {
            rpcView.CurrentView = RPCView.ViewType.Default;
            PartialUpdate();
        }*/

        /// <summary>
        /// Partially updates UI elements (Start Btn text + style, rConnectStatus and rConnectStatus)
        /// </summary>
        /*private async void PartialUpdate()
        {
            btnStart.Content = await StartText();
            UpdateButtons();
            rConnectStatus.Text = GetLineFromLanguageFile(
                RpcClient.Status == ConnectionStatus.Connecting ? "Loading" : RpcClient.Status.ToString());
        }

        //Updates the Start button when we go to another RPC Page
        private async void RpcPageManager_NewCurrentPage(object sender, IRpcPage e)
        {
            btnStart.Content = await StartText();
            UpdateButtons();
        }

        private async Task<string> StartText() 
        {
            if (RpcClient.IsRunning)
            {
                return GetLineFromLanguageFile("Shutdown");
            }
            return $"{GetLineFromLanguageFile("Start")} {GetLineFromLanguageFile(RpcPageManager.CurrentPage?.LocalizableName)}";
        }*/

        public override async void UpdateText() => 
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
        {
            /*btnStart.Content = StartText();
            btnUpdatePresence.Content = GetLineFromLanguageFile("UpdatePresence");

            rStatus.Text = GetLineFromLanguageFile("Status") + ": ";
            rConnectStatus.Text = GetLineFromLanguageFile(
                RpcClient.Status == ConnectionStatus.Connecting ? "Loading" : RpcClient.Status.ToString());
            rUser.Text = GetLineFromLanguageFile("User") + ": ";
            rUsername.Text = "Azy#0000"; //TODO: Store and get username
            tblAfk.Text = GetLineFromLanguageFile("AfkText") + ": ";

            btnAuto.Content = GetLineFromLanguageFile("Auto");
            btnAfk.Content = GetLineFromLanguageFile("Afk");*/
        });

        /*public void btnUpdatePresence_Click(object sender, RoutedEventArgs e) => 
            RpcClient?.UpdatePresence(RpcPageManager.CurrentPage.RichPresence);

        public void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!RpcClient.IsRunning)
            {
                RpcClient.Start(RpcPageManager.CurrentPage.RichPresence.ApplicationId);
            }
            else
            {
                RpcClient.Stop();
            }
        }*/

        public void btnAfk_Click(object sender, RoutedEventArgs e)
        {
            /*if (RpcClient.IsRunning && RpcClient.ActivePresence.ApplicationId != AfkPresence.ApplicationId)
            {
                RpcClient.Stop();
            }*/
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

        /*private bool ButtonEnabled => MainPage.ActivePage == RpcPageManager.CurrentPage && RpcPageManager.CurrentPage.VaildRichPresence;

        private async void UpdateButtons()
        {
            //TODO: Find out how to do this without removing the style completely
            btnUpdatePresence.Style = RpcClient.IsRunning ? (Style)Application.Current.Resources["btnPurple"] : null;
            btnUpdatePresence.IsEnabled = RpcClient.IsRunning && ButtonEnabled;

            btnStart.Style = (Style)Application.Current.Resources[RpcClient.IsRunning ? "btnRed" : "btnGreen"];
            btnStart.Content = await StartText();
            btnStart.IsEnabled = RpcClient.IsRunning || RpcPageManager.CurrentPage.VaildRichPresence;
        }*/
    }
}
