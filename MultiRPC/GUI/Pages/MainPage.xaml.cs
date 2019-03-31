using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private Button SelectedButton;
        private static DiscordRPC.RichPresence PresenceBeforeAfk;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            butStart.Click += ButStart_OnClick;

            rUsername.Text = App.Config.LastUser;
            mainPage = this;

            butMuiltiRPC.Tag = new MultiRPCPage();
            butCustom.Tag = new CustomPage();
            butLogs.Tag = new LogsPage();
            butCredits.Tag = new CreditsPage();
            butSettings.Tag = new SettingsPage();
            butDebug.Tag = new DebugPage();
            butMuiltiRPC.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            frameRPCPreview.Content = new RPCPreview(RPCPreview.ViewType.Default);
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
            if (!App.Config.DiscordCheck)
            {
                spCheckForDiscord.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                tblMadeBy.Text = $"{App.Text.MadeBy}: {App.AppDev}";
                rDiscordServer.Text = App.Text.DiscordServer + ": ";
                rServerLink.Text = App.ServerInviteCode;
                hylServerLinkUri.NavigateUri = new Uri($"https://discord.gg/{App.ServerInviteCode}");

                int count;
                string DiscordClient = "";
                try
                {
                    FindClient:
                    Process[] Client = Process.GetProcessesByName("Discord");
                    if ((count = Client.Length) != 0)
                    {
                        tblMultiRPC.Text = "MultiRPC - Discord";
                        DiscordClient = "Discord";
                    }
                    else
                    {
                        Client = Process.GetProcessesByName("DiscordCanary");
                        if ((count = Client.Length) != 0)
                        {
                            tblMultiRPC.Text = "MultiRPC - Discord Canary";
                            DiscordClient = "Discord Canary";
                        }
                        else
                        {
                            Client = Process.GetProcessesByName("DiscordPTB");
                            if ((count = Client.Length) != 0)
                            {
                                tblMultiRPC.Text = "MultiRPC - Discord PTB";
                                DiscordClient = "Discord PTB";
                            }
                        }
                    }

                    if (count == 0)
                    {
                        tblDiscordClientMessage.Text = App.Text.CantFindDiscord;
                        await Task.Delay(750);
                        goto FindClient;
                    }
                    else if (count < 6)
                    {
                        tblDiscordClientMessage.Text = $"{DiscordClient} {App.Text.IsLoading}....";
                        await Task.Delay(750);
                        goto FindClient;
                    }
                    else
                        spCheckForDiscord.Visibility = Visibility.Collapsed;
                }
                catch { }
                Updater.Check();
            }
        }

        public static MainPage mainPage;
        public static MainPage ThisPage => mainPage; 

        public async Task UpdateText()
        {
            if (ContentFrame.Content is MultiRPCPage)
                butStart.Content = $"{App.Text.Start} MuiltiRPC";
            else if (ContentFrame.Content is CustomPage)
                butStart.Content = App.Text.StartCustom;
            
            butUpdate.Content = App.Text.UpdatePresence;
            rStatus.Text = App.Text.Status + ": ";
            rCon.Text = App.Text.Disconnected;
            rUser.Text = App.Text.User + ": ";
            butAuto.Content = App.Text.Auto;
            butAfk.Content = App.Text.Afk;
            tblAfkText.Text = App.Text.AfkText + ": ";
        }

        private void ChangePage_OnClick(object sender, RoutedEventArgs e)
        {
            string ImageName(string s, bool Selected = false)
            {
                return s.Replace("img", "") + (Selected ? "Selected" : "") + "DrawingImage";
            }

            if (SelectedButton != null)
            {
                SelectedButton.Style = (Style)this.Resources["NavButton"];
                ((Image) SelectedButton.Content).Source = (DrawingImage) App.Current.Resources[ImageName(((Image)SelectedButton.Content).Name)];
            }
            SelectedButton = (Button)sender;

            SelectedButton.Style = (Style)this.Resources["NavButtonSelected"];
            ((Image)SelectedButton.Content).Source = (DrawingImage)App.Current.Resources[ImageName(((Image)SelectedButton.Content).Name, true)];
            ContentFrame.Navigate(SelectedButton.Tag);
        }

        public async Task RerenderButtons()
        {
            string ImageName(Button but)
            {
                var s = ((Image)but.Content).Name.Replace("img", "") + (but == SelectedButton ? "Selected" : "") +
                        "DrawingImage";
                return s;
            }

            ((Image)butMuiltiRPC.Content).Source = (DrawingImage)App.Current.Resources[ImageName(butMuiltiRPC)];
            ((Image)butCustom.Content).Source = (DrawingImage)App.Current.Resources[ImageName(butCustom)];
            ((Image)butLogs.Content).Source = (DrawingImage)App.Current.Resources[ImageName(butLogs)];
            ((Image)butSettings.Content).Source = (DrawingImage)App.Current.Resources[ImageName(butSettings)];
            ((Image)butCredits.Content).Source = (DrawingImage)App.Current.Resources[ImageName(butCredits)];
            ((Image)butDebug.Content).Source = (DrawingImage)App.Current.Resources[ImageName(butDebug)];
        }

        public async Task CanRunRPC()
        {
            if (tblAfkReason.Text.Length == 1)
            {
                tblAfkReason.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tblAfkReason.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                butAfk.IsEnabled = false;
            }
            else
            {
                tblAfkReason.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tblAfkReason.ToolTip = null;
                butAfk.IsEnabled = true;
            }
        }

        private static void ButStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (((Button) sender).Content.ToString() != App.Text.Shutdown)
            {
                RPC.Start();
                ThisPage.butAfk.IsEnabled = false;
            }
            else
            {
                RPC.Shutdown();
                if(PresenceBeforeAfk != null)
                    RPC.Presence = PresenceBeforeAfk;
                ThisPage.butAfk.IsEnabled = true;
            }
        }

        private void ButUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.Update();
        }

        private async void ButAfk_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tblAfkReason.Text))
            {
                if (PresenceBeforeAfk == null)
                    PresenceBeforeAfk = RPC.Presence.Clone();
                RPC.SetPresence(tblAfkReason.Text, "", "cat", App.Text.SleepyCat, "", "", App.Config.AFKTime);
                RPC.AFK = true;
                var tmp = RPC.IDToUse;
                RPC.IDToUse = 469643793851744257;
                tblAfkReason.Text = "";
                RPC.Update();
                RPC.IDToUse = tmp;
                RPC.Presence = PresenceBeforeAfk.Clone();
            }
            else
            {
                await CustomMessageBox.Show(App.Text.NeedAfkReason);
            }
        }

        private void TblAfkReason_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CanRunRPC();
        }

        private void HylServerLinkUri_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("https://discord.gg/" + App.ServerInviteCode);
            e.Handled = true;
        }
    }
}
