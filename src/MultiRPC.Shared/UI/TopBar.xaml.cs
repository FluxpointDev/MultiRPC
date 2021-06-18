using System;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using MultiRPC.Core.Pages;
using Microsoft.Extensions.DependencyInjection;
using DiscordRPC;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiRPC.Shared.UI.Views;
using DiscordRPC.Message;
using Serilog;
using static MultiRPC.Core.LanguagePicker;
using RichPresence = MultiRPC.Core.Rpc.RichPresence;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopBar : LocalizablePage
    {
        public MainPage MainPage = null!;

        private readonly RichPresence AfkPresence = new("Afk", Constants.AfkID)
        {
            Presence = new DiscordRPC.RichPresence
            {
                Assets = new Assets
                {
                    LargeImageKey = "cat"
                }
            }
        };

        private readonly RpcClient RpcClient;

        public TopBar()
        {
            RpcClient = ServiceManager.ServiceProvider.GetRequiredService<RpcClient>();

            RpcClient.Disconnected += RpcClient_Disconnected;
            RpcClient.Ready += RpcClient_Ready;
            RpcClient.Loading += RpcClient_Loading;
            RpcClient.Errored += (_, e) => rpcView.CurrentView = RPCView.ViewType.Error;
            RpcClient.PresenceUpdated += (_, e) => rpcView.RichPresence = e;

            RpcPageManager.PageChanged += RpcPageManager_NewCurrentPage;
            RpcPageManager.PagePropertyChanged += (_, __ ) => UpdateButtons();

            Loaded += TopBar_Loaded;
            InitializeComponent();
        }

        private void TopBar_Loaded(object sender, RoutedEventArgs e)
        {
            //Got to do it here as the MainPage won't be made until the application has loaded
            MainPage = ServiceManager.ServiceProvider.GetRequiredService<MainPage>();
            Loaded -= TopBar_Loaded;
        }

        //To update the RPC View's presence
        private void RpcClient_Loading(object sender, EventArgs e)
        {
            rpcView.CurrentView = RPCView.ViewType.Loading;
            PartialUpdate();
        }

        private void RpcClient_Ready(object sender, ReadyMessage e)
        {
            rpcView.CurrentView = RPCView.ViewType.RichPresence;
            UpdateText();
        }

        private void RpcClient_Disconnected(object sender, EventArgs e)
        {
            rpcView.CurrentView = RPCView.ViewType.Default;
            PartialUpdate();
        }

        /// <summary>
        /// Partially updates UI elements (Start Btn text + style, rConnectStatus and rConnectStatus)
        /// </summary>
        private void PartialUpdate()
        {
            btnStart.Content = StartText();
            UpdateButtons();
            rConnectStatus.Text = GetLineFromLanguageFile(
                RpcClient.Status == ConnectionStatus.Connecting ? "Loading" : RpcClient.Status.ToString());
        }

        //Updates the Start button when we go to another RPC Page
        private void RpcPageManager_NewCurrentPage(object sender, IRpcPage e)
        {
            btnStart.Content = StartText();
            UpdateButtons();
        }

        private string StartText()
        {
            if (RpcClient.IsRunning)
            {
                return GetLineFromLanguageFile("Shutdown");
            }
            return $"{GetLineFromLanguageFile("Start")} {GetLineFromLanguageFile(RpcPageManager.CurrentPage?.LocalizableName ?? "NA")}";
        }

        public override void UpdateText()
        {
            this.RunLogic(() => updateText());
        }

        private void updateText()
        {
            btnStart.Content = StartText();
            btnUpdatePresence.Content = GetLineFromLanguageFile("UpdatePresence");

            rStatus.Text = GetLineFromLanguageFile("Status") + ": ";
            rConnectStatus.Text = GetLineFromLanguageFile(
                RpcClient.Status == ConnectionStatus.Connecting ? "Loading" : RpcClient.Status.ToString());
            rUser.Text = GetLineFromLanguageFile("User") + ": ";
            rUsername.Text = "Azy#0000"; //TODO: Store and get username
            tblAfk.Text = GetLineFromLanguageFile("AfkText") + ": ";

            btnAuto.Content = GetLineFromLanguageFile("Auto");
            btnAfk.Content = GetLineFromLanguageFile("Afk");
        }

        public void btnUpdatePresence_Click(object sender, RoutedEventArgs e) => 
            RpcClient?.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);

        public void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!RpcClient.IsRunning)
            {
                RpcClient.Start(null, null);
            }
            else
            {
                RpcClient.Stop();
            }
        }

        public void btnAfk_Click(object sender, RoutedEventArgs e)
        {
            if (RpcClient.IsRunning
                && RpcClient.ActivePresence.ID != AfkPresence.ID)
            {
                RpcClient.Stop();
            }
            if (!RpcClient.IsRunning)
            {
                RpcClient.Start(AfkPresence.ID, AfkPresence.Name);
            }

            AfkPresence.Presence.Details = txtAfk.Text;
            RpcClient.UpdatePresence(AfkPresence);
        }

        private void txtAfk_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnAfk.IsEnabled = txtAfk.Text.Length != 1;
        }

        private bool PresenceButtonEnabled =>
            RpcClient.IsRunning
            && MainPage.ActivePage == RpcPageManager.CurrentPage
            && RpcPageManager.CurrentPage.VaildRichPresence;

        private void UpdateButtons()
        {
            btnStart.Content = StartText();
            if (RpcClient.IsRunning)
            {
                btnUpdatePresence.SetValue(StyleProperty, Application.Current.Resources["btnPurple"]);
                btnStart.SetValue(StyleProperty, Application.Current.Resources["btnRed"]);
                btnStart.IsEnabled = true;
                return;
            }

            btnUpdatePresence.SetValue(StyleProperty, null);
            btnStart.SetValue(StyleProperty, Application.Current.Resources["btnGreen"]);
            btnStart.IsEnabled = RpcPageManager.CurrentPage?.VaildRichPresence ?? false;
        }
    }
}