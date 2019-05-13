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
        private Button selectedButton;
        private static DiscordRPC.RichPresence presenceBeforeAfk;
        public static MainPage _MainPage;

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            btnStart.Click += ButStart_OnClick;

            rUsername.Text = App.Config.LastUser;
            _MainPage = this;

            frameRPCPreview.Content = new RPCPreview(RPCPreview.ViewType.Default);
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
            Updater.Check();
            if (!App.Config.DiscordCheck)
            {
                if (App.Config.AutoStart == "MultiRPC" || App.Config.AutoStart == App.Text.No)
                {
                    btnMultiRPC.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                else
                {
                    btnCustom.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
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
                string discordClient = "";
                try
                {
                    FindClient:
                    Process[] client = Process.GetProcessesByName("Discord");
                    if ((count = client.Length) != 0)
                    {
                        tblMultiRPC.Text = "MultiRPC - Discord";
                        discordClient = "Discord";
                    }
                    else
                    {
                        client = Process.GetProcessesByName("DiscordCanary");
                        if ((count = client.Length) != 0)
                        {
                            tblMultiRPC.Text = "MultiRPC - Discord Canary";
                            discordClient = "Discord Canary";
                        }
                        else
                        {
                            client = Process.GetProcessesByName("DiscordPTB");
                            if ((count = client.Length) != 0)
                            {
                                tblMultiRPC.Text = "MultiRPC - Discord PTB";
                                discordClient = "Discord PTB";
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
                        tblDiscordClientMessage.Text = $"{discordClient} {App.Text.IsLoading}....";
                        await Task.Delay(750);
                        goto FindClient;
                    }
                    else
                    {
                        if (App.Config.AutoStart == "MultiRPC" || App.Config.AutoStart == App.Text.No)
                        {
                            btnMultiRPC.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        else
                        {
                            btnCustom.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        gridCheckForDiscord.Visibility = Visibility.Collapsed;
                    }
                }
                catch { }
            }
        }

        public Task UpdateText()
        {
            switch (ContentFrame.Content)
            {
                case MultiRPCPage _:
                    btnStart.Content = $"{App.Text.Start} MuiltiRPC";
                    break;
                case CustomPage _:
                    btnStart.Content = App.Text.StartCustom;
                    break;
            }
            
            btnUpdate.Content = App.Text.UpdatePresence;
            rStatus.Text = App.Text.Status + ": ";
            rCon.Text = App.Text.Disconnected;
            rUser.Text = App.Text.User + ": ";
            btnAuto.Content = App.Text.Auto;
            btnAfk.Content = App.Text.Afk;
            tblAfkText.Text = App.Text.AfkText + ": ";
            return Task.CompletedTask;
        }

        private void ChangePage_OnClick(object sender, RoutedEventArgs e)
        {
            if (selectedButton?.Tag != null && ContentFrame.Content == ((Button)sender).Tag)
            {
                return;
            }

            string ImageName(string s, bool selected = false)
            {
                return s.Replace("img", "") + (selected ? "Selected" : "") + "DrawingImage";
            }

            if (selectedButton != null)
            {
                selectedButton.SetResourceReference(StyleProperty, "NavButton");
                ((Image)selectedButton.Content).Source = (DrawingImage) App.Current.Resources[ImageName(((Image)selectedButton.Content).Name)];
            }
            selectedButton = (Button)sender;

            selectedButton.SetResourceReference(StyleProperty, "NavButtonSelected");
            ((Image)selectedButton.Content).Source = (DrawingImage)App.Current.Resources[ImageName(((Image)selectedButton.Content).Name, true)];
            if (selectedButton.Tag == null)
            {
                switch (((Button)sender).Name)
                {
                    case "btnMultiRPC":
                        btnMultiRPC.Tag = new MultiRPCPage();
                        break;
                    case "btnCustom":
                        btnCustom.Tag = new CustomPage();
                        break;
                    case "btnLogs":
                        btnLogs.Tag = new LogsPage();
                        break;
                    case "btnCredits":
                        btnCredits.Tag = new CreditsPage();
                        break;
                    case "btnSettings":
                        btnSettings.Tag = new SettingsPage();
                        break;
                    case "btnDebug":
                        btnDebug.Tag = new DebugPage();
                        break;
                    case "btnThemeEditor":
                        btnThemeEditor.Tag = new ThemeEditorPage();
                        break;
                }
            }

            ContentFrame.Navigate(selectedButton.Tag);
        }

        public Task RerenderButtons()
        {
            string ImageName(Button but)
            {
                var s = ((Image)but.Content).Name.Replace("img", "") + (but == selectedButton ? "Selected" : "") +
                        "DrawingImage";
                return s;
            }

            ((Image)btnMultiRPC.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnMultiRPC)];
            ((Image)btnCustom.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnCustom)];
            ((Image)btnLogs.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnLogs)];
            ((Image)btnSettings.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnSettings)];
            ((Image)btnCredits.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnCredits)];
            ((Image)btnDebug.Content).Source = (DrawingImage)App.Current.Resources[ImageName(btnDebug)];
            return Task.CompletedTask;
        }

        private Task CanRunRPC()
        {
            if (tbAfkReason.Text.Length == 1)
            {
                tbAfkReason.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbAfkReason.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                btnAfk.IsEnabled = false;
            }
            else
            {
                tbAfkReason.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbAfkReason.ToolTip = null;
                btnAfk.IsEnabled = true;
            }
            return Task.CompletedTask;
        }

        private static void ButStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (((Button) sender).Content.ToString() != App.Text.Shutdown)
            {
                RPC.Start();
                _MainPage.btnAfk.IsEnabled = false;
            }
            else
            {
                RPC.Shutdown();
                if (presenceBeforeAfk != null)
                    RPC.Presence = presenceBeforeAfk;
                _MainPage.btnAfk.IsEnabled = true;

                if (_MainPage.ContentFrame.Content is MultiRPCPage mainRpcPage)
                {
                    RPC.UpdateType(RPC.RPCType.MultiRPC);
                    mainRpcPage.CanRunRPC();
                }
                else if (_MainPage.ContentFrame.Content is CustomPage customPage)
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
                if (presenceBeforeAfk == null)
                    presenceBeforeAfk = RPC.Presence.Clone();
                RPC.AFK = true;

                var tmp = RPC.IDToUse;
                RPC.IDToUse = 469643793851744257;
                RPC.SetPresence(tbAfkReason.Text, "", "cat", App.Text.SleepyCat, "", "", App.Config.AFKTime);
                RPC.Update();
                tbAfkReason.Text = "";
                RPC.IDToUse = tmp;
                await Task.Delay(100);
                RPC.Presence = presenceBeforeAfk.Clone();
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
