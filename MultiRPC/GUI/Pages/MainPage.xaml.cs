using System;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
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
            btnStart.Click += ButStart_OnClick;

            rUsername.Text = App.Config.LastUser;
            _mainPage = this;

            btnMuiltiRPC.Tag = new MultiRPCPage();
            btnCustom.Tag = new CustomPage();
            btnLogs.Tag = new LogsPage();
            btnCredits.Tag = new CreditsPage();
            btnSettings.Tag = new SettingsPage();
            btnDebug.Tag = new DebugPage();
            btnThemeEditor.Tag = new ThemeEditorPage();
            btnMuiltiRPC.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            frameRPCPreview.Content = new RPCPreview(RPCPreview.ViewType.Default);
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
            if (!App.Config.DiscordCheck)
            {
                gridCheckForDiscord.Visibility = Visibility.Collapsed;
            }
            else
            {
                tblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
                        gridCheckForDiscord.Visibility = Visibility.Collapsed;
                }
                catch { }
                Updater.Check();
            }
        }

        public static MainPage _mainPage;
        public static MainPage mainPage => _mainPage; 

        public async Task UpdateText()
        {
            if (ContentFrame.Content is MultiRPCPage)
                btnStart.Content = $"{App.Text.Start} MuiltiRPC";
            else if (ContentFrame.Content is CustomPage)
                btnStart.Content = App.Text.StartCustom;
            
            btnUpdate.Content = App.Text.UpdatePresence;
            rStatus.Text = App.Text.Status + ": ";
            rCon.Text = App.Text.Disconnected;
            rUser.Text = App.Text.User + ": ";
            btnAuto.Content = App.Text.Auto;
            btnAfk.Content = App.Text.Afk;
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

            ((Image)btnMuiltiRPC.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnMuiltiRPC)];
            ((Image)btnCustom.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnCustom)];
            ((Image)btnLogs.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnLogs)];
            ((Image)btnSettings.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnSettings)];
            ((Image)btnCredits.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnCredits)];
            ((Image)btnDebug.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnDebug)];
        }

        public async Task CanRunRPC()
        {
            if (tbAfkReason.Text.Length == 1)
            {
                tbAfkReason.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbAfkReason.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                btnAfk.IsEnabled = false;
            }
            else
            {
                tbAfkReason.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbAfkReason.ToolTip = null;
                btnAfk.IsEnabled = true;
            }
        }

        private static void ButStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (((Button) sender).Content.ToString() != App.Text.Shutdown)
            {
                RPC.Start();
                mainPage.btnAfk.IsEnabled = false;
            }
            else
            {
                RPC.Shutdown();
                if(PresenceBeforeAfk != null)
                    RPC.Presence = PresenceBeforeAfk;
                mainPage.btnAfk.IsEnabled = true;

                if (mainPage.ContentFrame.Content is MultiRPCPage mainRpcPage)
                {
                    RPC.UpdateType(RPC.RPCType.MultiRPC);
                    mainRpcPage.CanRunRPC();
                }
                else if (mainPage.ContentFrame.Content is CustomPage customPage)
                {
                    RPC.UpdateType(RPC.RPCType.Custom);
                    customPage.CanRunRPC(true);
                }
            }
        }

        private void ButUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.Update();
        }

        private async void ButAfk_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbAfkReason.Text))
            {
                if (PresenceBeforeAfk == null)
                    PresenceBeforeAfk = RPC.Presence.Clone();
                RPC.SetPresence(tbAfkReason.Text, "", "cat", App.Text.SleepyCat, "", "", App.Config.AFKTime);
                RPC.AFK = true;
                var tmp = RPC.IDToUse;
                RPC.IDToUse = 469643793851744257;
                tbAfkReason.Text = "";
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
