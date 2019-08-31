using System;
using System.Diagnostics;
using System.Extra;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public static MainPage _MainPage;
        private Button _selectedButton;

        public MainPage()
        {
            InitializeComponent();

            rUsername.Text = App.Config.LastUser;
            tblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _MainPage = this;

            frmRPCPreview.Content = new RPCPreview(RPCPreview.ViewType.Default);
            UpdateText();
#if DEBUG
            btnPrograms.Visibility = Visibility.Visible;
#endif
            NetworkChange.NetworkAddressChanged += AddressChangedCallback;
            AddressChangedCallback(null, null);
        }

        private async void AddressChangedCallback(object sender, EventArgs e)
        {
            if (Utils.NetworkIsAvailable())
            {
                await gridInternetConnectivity.Dispatcher.InvokeAsync(async () =>
                {
                    gridInternetConnectivity.SetResourceReference(Panel.BackgroundProperty, "Green");
                    tblInternetConnectivity.Text = App.Text.InternetBack + "!!";
                    await Task.Delay(3000);
                    gridInternetConnectivity.Height = 0;
                });
            }
            else
            {
                await gridInternetConnectivity.Dispatcher.InvokeAsync(() =>
                {
                    gridInternetConnectivity.Height = double.NaN;
                    tblInternetConnectivity.Text = App.Text.InternetLost + "!!";
                    gridInternetConnectivity.SetResourceReference(Panel.BackgroundProperty, "Red");
                });
            }
        }

        private async void DiscordCheckFadeOutAnimation()
        {
            if (App.Config.AutoStart == "MultiRPC" || App.Config.AutoStart == App.Text.No)
            {
                btnMultiRPC.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                btnCustom.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }

            await Animations.DoubleAnimation(gridCheckForDiscord, 0, gridCheckForDiscord.Opacity);
            gridCheckForDiscord.Visibility = Visibility.Collapsed;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime startDateTime;

            async void ShowTempDisableButton()
            {
                if (btnDisableDiscordCheck.Opacity == 0 && DateTime.Now.Subtract(startDateTime).TotalSeconds >= 5)
                {
                    await Animations.DoubleAnimation(btnDisableDiscordCheck, 1, btnDisableDiscordCheck.Opacity);
                }
            }

            Updater.Check();
            if (!App.Config.DiscordCheck)
            {
                DiscordCheckFadeOutAnimation();
            }
            else
            {
                tblMadeBy.Text = $"{App.Text.MadeBy}: {App.AppDev}";
                rDiscordServer.Text = App.Text.DiscordServer + ": ";
                rServerLink.Text = App.ServerInviteCode;
                hylServerLinkUri.NavigateUri = new[] { "https://discord.gg", App.ServerInviteCode }.CombineToUri();
                startDateTime = DateTime.Now;

                int processCount;
                var discordClient = "";
                try
                {
                    FindClient:
                    var client = Process.GetProcessesByName("Discord");
                    if ((processCount = client.Length) != 0)
                    {
                        tblMultiRPC.Text = "MultiRPC - Discord";
                        discordClient = "Discord";
                    }
                    else
                    {
                        client = Process.GetProcessesByName("DiscordCanary");
                        if ((processCount = client.Length) != 0)
                        {
                            tblMultiRPC.Text = "MultiRPC - Discord Canary";
                            discordClient = "Discord Canary";
                        }
                        else
                        {
                            client = Process.GetProcessesByName("DiscordPTB");
                            if ((processCount = client.Length) != 0)
                            {
                                tblMultiRPC.Text = "MultiRPC - Discord PTB";
                                discordClient = "Discord PTB";
                            }
                        }
                    }

                    if (processCount == 0)
                    {
                        tblDiscordClientMessage.Text = App.Text.CantFindDiscord;
                        ShowTempDisableButton();
                        await Task.Delay(750);
                        goto FindClient;
                    }

                    if (processCount < 4)
                    {
                        tblDiscordClientMessage.Text = $"{discordClient} {App.Text.IsLoading}....";
                        ShowTempDisableButton();
                        await Task.Delay(750);
                        goto FindClient;
                    }

                    DiscordCheckFadeOutAnimation();
                }
                catch
                {
                    App.Logging.Application(App.Text.ProcessFindError);
                    DiscordCheckFadeOutAnimation();
                }
            }
        }

        public Task UpdateTooltips()
        {
            btnMultiRPC.ToolTip = !App.Config.ShowPageTooltips ? null : new ToolTip("MultiRPC" + " " + App.Text.Page);
            btnCustom.ToolTip =
                !App.Config.ShowPageTooltips ? null : new ToolTip(App.Text.Custom + " " + App.Text.Page);
            btnSettings.ToolTip = !App.Config.ShowPageTooltips
                ? null
                : new ToolTip(App.Text.Settings + " " + App.Text.Page);
            btnLogs.ToolTip = !App.Config.ShowPageTooltips ? null : new ToolTip(App.Text.Log + " " + App.Text.Page);
            btnCredits.ToolTip = !App.Config.ShowPageTooltips
                ? null
                : new ToolTip(App.Text.Credits + " " + App.Text.Page);
            btnThemeEditor.ToolTip =
                !App.Config.ShowPageTooltips ? null : new ToolTip(App.Text.Theme + " " + App.Text.Page);
            btnDebug.ToolTip = !App.Config.ShowPageTooltips ? null : new ToolTip(App.Text.Debug + " " + App.Text.Page);
            btnPrograms.ToolTip = !App.Config.ShowPageTooltips
                ? null
                : new ToolTip(App.Text.Programs + " " + App.Text.Page);
            return Task.CompletedTask;
        }

        public Task UpdateText()
        {
            if (btnStart.Background != Application.Current.Resources["Red"])
            {
                switch (frmContent.Content)
                {
                    case MultiRPCPage _:
                        btnStart.Content = $"{App.Text.Start} MultiRPC";
                        break;
                    case MasterCustomPage _:
                        btnStart.Content = App.Text.StartCustom;
                        break;
                    default:
                        if (btnStart.Content != null)
                        {
                            btnStart.Content = btnStart.Content.ToString().Contains("MultiRPC")
                                ? $"{App.Text.Start} MultiRPC"
                                : App.Text.StartCustom;
                        }

                        break;
                }
            }
            else
            {
                btnStart.Content = App.Text.Shutdown;
            }

            btnUpdate.Content = App.Text.UpdatePresence;
            rStatus.Text = App.Text.Status + ": ";
            rCon.Text = App.Text.Disconnected;
            rUser.Text = App.Text.User + ": ";
            btnAuto.Content = App.Text.Auto;
            btnAfk.Content = App.Text.Afk;
            tblAfkText.Text = App.Text.AfkText + ": ";
            btnDisableDiscordCheck.Content = App.Text.TempDisableDiscordCheck;
            var preview = (RPCPreview) frmRPCPreview.Content;
            if (preview != null && preview.CurrentViewType != RPCPreview.ViewType.RichPresence)
            {
                preview.UpdateUIViewType(preview.CurrentViewType);
            }

            UpdateTooltips();

            return Task.CompletedTask;
        }

        private void ChangePage_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedButton?.Tag != null && frmContent.Content == ((Button) sender).Tag)
            {
                return;
            }

            string ImageName(string s, bool selected = false)
            {
                return s.Replace("img", "") + (selected ? "Selected" : "") + "DrawingImage";
            }

            if (_selectedButton != null)
            {
                _selectedButton.SetResourceReference(StyleProperty, "NavButton");
                ((Image) _selectedButton.Content).Source =
                    (DrawingImage) Application.Current.Resources[ImageName(((Image) _selectedButton.Content).Name)];
            }

            _selectedButton = (Button) sender;

            _selectedButton.SetResourceReference(StyleProperty, "NavButtonSelected");
            ((Image) _selectedButton.Content).Source =
                (DrawingImage) Application.Current.Resources[ImageName(((Image) _selectedButton.Content).Name, true)];

            var pageWidth = btnMultiRPC.Tag != null ? ((MultiRPCPage) btnMultiRPC.Tag).ActualWidth : ActualWidth;

            if (_selectedButton.Tag == null)
            {
                switch (((Button) sender).Name)
                {
                    case "btnMultiRPC":
                        btnMultiRPC.Tag = new MultiRPCPage();
                        break;
                    case "btnCustom":
                        btnCustom.Tag = new MasterCustomPage(pageWidth);
                        break;
                    case "btnLogs":
                        btnLogs.Tag = new LogsPage(pageWidth);
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
                        btnThemeEditor.Tag = new MasterThemeEditorPage();
                        break;
                    case "btnPrograms":
                        btnPrograms.Tag = new ProgramsPage();
                        break;
                }
            }

            //var drawingImage = new DrawingImage();
            //var image = new ImageDrawing();
            //var imageBitmap = new BitmapImage(new System.Uri("Assets/150x150Logo.png", UriKind.Relative));
            //image.ImageSource = imageBitmap;
            //image.Rect = new Rect(new Size(imageBitmap.Width, imageBitmap.Height));
            //drawingImage.Drawing = image;

            //((Image) _selectedButton.Content).Source = drawingImage;

            frmContent.Navigate(_selectedButton.Tag);
        }

        public Task RerenderButtons()
        {
            string ImageName(Button but)
            {
                return ((Image) but.Content).Name.Replace("img", "") + (but == _selectedButton ? "Selected" : "") +
                       "DrawingImage";
            }

            ((Image) btnMultiRPC.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnMultiRPC)];
            ((Image) btnCustom.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnCustom)];
            ((Image) btnLogs.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnLogs)];
            ((Image) btnSettings.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnSettings)];
            ((Image) btnCredits.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnCredits)];
            ((Image) btnDebug.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnDebug)];
            ((Image) btnPrograms.Content).Source = (DrawingImage) Application.Current.Resources[ImageName(btnPrograms)];

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

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() != App.Text.Shutdown)
            {
                RPC.Start();
                _MainPage.btnAfk.IsEnabled = false;
            }
            else
            {
                RPC.Shutdown();
                _MainPage.btnAfk.IsEnabled = true;

                if (_MainPage.frmContent.Content is MultiRPCPage mainRpcPage)
                {
                    RPC.UpdateType(RPC.RPCType.MultiRPC);
                    RPC.IDToUse = RPC.MultiRPCID;
                    mainRpcPage.TbText1_OnTextChanged(mainRpcPage.tbText1, null);
                    mainRpcPage.CanRunRPC();
                }
                else if (_MainPage.frmContent.Content is MasterCustomPage masterCustomPage)
                {
                    RPC.UpdateType(RPC.RPCType.Custom);
                    var customPage = masterCustomPage.CustomPage;
                    customPage.TbText1_OnTextChanged(customPage.tbText1, null);
                    customPage.CanRunRPC(true);
                }
            }
        }

        private void BtnUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.Update();
        }

        private async void BtnAfk_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbAfkReason.Text))
            {
                RPC.SetPresence(tbAfkReason.Text, "", "cat", App.Text.SleepyCat, "", "", App.Config.AFKTime);
                RPC.AFK = true;

                if (RPC.IDToUse != 469643793851744257)
                {
                    RPC.Shutdown();
                }

                RPC.IDToUse = 469643793851744257;
                RPC.Update();
                tbAfkReason.Text = "";
                btnStart.IsEnabled = true;
                btnUpdate.IsEnabled = false;
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
            Process.Start(new [] { "https://discord.gg", App.ServerInviteCode }.Combine());
            e.Handled = true;
        }

        private void ChangePage_OnMouseDown(object sender, MouseEventArgs e)
        {
            var button = (Button) sender;
            Animations.ThicknessAnimation(button, new Thickness(2), button.Margin);
        }

        private void ChangePage_OnMouseUp(object sender, MouseEventArgs e)
        {
            var button = (Button) sender;
            Animations.ThicknessAnimation(button, new Thickness(0), button.Margin, ease: new BounceEase());
        }

        private void BtnDisableDiscordCheck_OnClick(object sender, RoutedEventArgs e)
        {
            DiscordCheckFadeOutAnimation();
        }
    }
}