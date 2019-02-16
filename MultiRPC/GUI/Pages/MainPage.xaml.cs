using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MultiRPC.Functions;
using System.Deployment.Application;
using System.Net;
using MultiRPC.Data;
using System.Threading.Tasks;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public Window Window = null;

        public MainPage(Style ComboBoxStyle, Window window)
        {
            Window = window;
            Window.Closing += Window_Closing;
            Window.ContentRendered += Window_ContentRendered;

            InitializeComponent();
            TextVersion.Content = App.Version;
            IsEnabled = false;
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2)
            {
                Views.Default = new ViewDefault(ComboBoxStyle);
                ItemsPipe.Style = ComboBoxStyle;
                ItemsAutoStart.Style = ComboBoxStyle;
            }
            else
                Views.Default = new ViewDefault(null);
            FrameDefaultView.Content = Views.Default;
            FrameLiveRPC.Content = new ViewRPC(ViewType.Default);
            TabDebug.Visibility = Visibility.Hidden;
            if (!Directory.Exists(App.ConfigFolder))
                Directory.CreateDirectory(App.ConfigFolder);

            if (File.Exists(App.ConfigFile))
            {
                using (StreamReader reader = new StreamReader(App.ConfigFile))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    App.Config = (Config)serializer.Deserialize(reader, typeof(Config));
                }
            }

            try
            {
                Process[] Discord = Process.GetProcessesByName("Discord");
                if (Discord.Count() != 0)
                    Window.Title = "MultiRPC - Discord";
                else
                {
                    Process[] DiscordCanary = Process.GetProcessesByName("DiscordCanary");
                    if (DiscordCanary.Count() != 0)
                        Window.Title = "MultiRPC - Discord Canary";
                    else
                    {
                        Process[] DiscordPTB = Process.GetProcessesByName("DiscordPTB");
                        if (DiscordPTB.Count() != 0)
                            Window.Title = "MultiRPC - Discord PTB";
                    }
                }
            }
            catch { }
        }

        public TaskbarIcon Taskbar;

        /// <summary>
        /// When window loads check for update and configure the config
        /// </summary>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FuncUpdater.Check();
            if (!App.StartUpdate)
            {
                Taskbar = new TaskbarIcon
                {
                    IconSource = App.BW.Icon,
                    Name = "MultiRPC",
                    ToolTipText = "MultiRPC"
                };
                Taskbar.TrayLeftMouseDown += Taskbar_TrayLeftMouseDown;
                FuncDiscord.LoadPipes();
                _Data.SetupCustom(this);
                Views.Default.SetData();
                TextDev.Content = App.Developer;
                FileWatch.Create();
                if (File.Exists(App.ProfilesFile))
                {
                    using (StreamReader reader = new StreamReader(App.ProfilesFile))
                    {
                        JsonSerializer serializer = new JsonSerializer
                        {
                            Formatting = Formatting.Indented
                        };
                        _Data.Profiles = (Dictionary<string, CustomProfile>)serializer.Deserialize(reader, typeof(Dictionary<string, CustomProfile>));

                    }
                    if (_Data.Profiles.Count == 0)
                    {
                        CustomProfile profile = new CustomProfile { Name = "Custom" };
                        _Data.Profiles.Add("Custom", profile);
                        _Data.SaveProfiles();
                        Views.Custom = new ViewCustom(profile);
                        FrameCustomView.Content = Views.Custom;
                        (MenuProfiles.Items[0] as Button).Click += ProfileBtn_Click;
                    }
                    else
                    {
                        foreach (CustomProfile p in _Data.Profiles.Values)
                        {
                            if (Views.Custom == null)
                            {
                                Views.Custom = new ViewCustom(p);
                                FrameCustomView.Content = Views.Custom;
                                (MenuProfiles.Items[0] as Button).Name = p.Name;
                                (MenuProfiles.Items[0] as Button).Content = p.Name;
                                (MenuProfiles.Items[0] as Button).Click += ProfileBtn_Click;
                            }
                            else
                            {
                                Button btn = p.GetButton();
                                btn.Click += ProfileBtn_Click;
                                MenuProfiles.Items.Add(btn);
                            }
                        }
                    }
                    App.WD.ToggleMenu();
                    CheckProfileMenuWidth();
                }
                else
                {
                    CustomProfile profile = new CustomProfile { Name = "Custom" };
                    _Data.Profiles.Add("Custom", profile);
                    _Data.SaveProfiles();
                    Views.Custom = new ViewCustom(profile);
                    FrameCustomView.Content = Views.Custom;
                    (MenuProfiles.Items[0] as Button).Click += ProfileBtn_Click;
                }
                App.FormReady = true;
                if (!string.IsNullOrEmpty(App.Config.LastUser))
                    TextUser.Content = App.Config.LastUser;
                FuncCredits.Download();
                IsEnabled = true;
                _Data.AutoStart(this);
            }
        }

        private void Taskbar_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Hidden)
                Visibility = Visibility.Visible;
            else
                Visibility = Visibility.Hidden;
        }

        public void CheckProfileMenuWidth()
        {
            MenuProfiles.Margin = new Thickness(-20, 0, 0, 0);
            MenuProfiles.UpdateLayout();
            if (!MenuProfiles.HasOverflowItems)
                MenuProfiles.Margin = new Thickness(-20, 0, 20, 0);
            else
                MenuProfiles.Margin = new Thickness(-20, 0, 0, 0);
        }

        /// <summary>
        /// Update rpc view with presence data
        /// </summary>
        /// <param name="msg"></param>
        public static void SetLiveView(DiscordRPC.Message.PresenceMessage msg)
        {
            App.WD.FrameLiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                App.WD.FrameLiveRPC.Content = new ViewRPC(msg);
            });
        }

        /// <summary>
        /// Update roc view with default modes
        /// </summary>
        public static void SetLiveView(ViewType view, string error = "")
        {
            App.WD.FrameLiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                App.WD.FrameLiveRPC.Content = new ViewRPC(view, error);
            });
        }

        /// <summary>
        /// Start/Stop rpc
        /// </summary>
        public void BtnToggleRPC_Click(object sender, RoutedEventArgs e)
        {
            if (Views.Custom.TextClientID.IsEnabled)
            {
                if (Views.Custom == null)
                    return;
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    App.Log.Error("App", $"No network connection available");
                    App.WD.FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "No Network Connection!");
                    return;
                }
                if (ToggleDiscordCheck.IsChecked.Value && !FuncDiscord.CheckDiscordClient())
                    return;
                DisableElements();
                ulong ID = 0;
                if (RPC.Type == "default")
                {
                    ID = 450894077165043722;
                    RPC.CheckField(Views.Default.TextText1.Text);
                    RPC.CheckField(Views.Default.TextText2.Text);
                    string LKey = "";
                    string SKey = "";
                    if (Views.Default.ItemsLarge.SelectedIndex != -1)
                        LKey = (Views.Default.ItemsLarge.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                    if (Views.Default.ItemsSmall.SelectedIndex != -1)
                        SKey = (Views.Default.ItemsSmall.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                    RPC.SetPresence(Views.Default.TextText1.Text, Views.Default.TextText2.Text, LKey, Views.Default.TextLarge.Text, SKey, Views.Default.TextSmall.Text);
                    if (Views.Default.ToggleTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);

                }
                else if (RPC.Type == "custom")
                {
                    RPC.CheckField(Views.Custom.TextText1.Text);
                    RPC.CheckField(Views.Custom.TextText2.Text);
                    Views.Custom.TextClientID.Text = Views.Custom.TextClientID.Text.Replace(" ", "");
                    if (!ulong.TryParse(Views.Custom.TextClientID.Text, out ID))
                    {
                        EnableElements(true);
                        App.Log.Error("App", $"Client ID is invalid");
                        FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "Client ID is invalid");
                        return;
                    }
                    if (!ToggleTokenCheck.IsChecked.Value)
                    {
                        Dictionary<string, string> dict = new Dictionary<string, string>
                        {
                            { "client_id", ID.ToString() }
                        };
                        HttpResponseMessage T = null;
                        try
                        {
                            T = RPC.HttpClient.PostAsync("https://discordapp.com/api/oauth2/token/rpc", new FormUrlEncodedContent(dict)).GetAwaiter().GetResult();
                        }
                        catch
                        {
                            App.Log.Error("API", $"Could not connect to Discord API");
                            FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "Network issue!");
                            EnableElements(true);
                            return;
                        }

                        if (T.StatusCode == HttpStatusCode.BadRequest)
                        {
                            App.Log.Error("API", $"Client ID is invalid");
                            FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "Client ID is invalid!");
                            EnableElements(true);
                            return;
                        }
                        if (T.StatusCode != HttpStatusCode.InternalServerError)
                        {
                            string Response = T.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            App.Log.Error("API", $"API error {Response}");
                            FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "API Issue!");
                            EnableElements(true);
                            return;
                        }
                    }
                    RPC.SetPresence(Views.Custom.Profile);
                    if (Views.Custom.ToggleTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                }
                FrameLiveRPC.Content = new ViewRPC(ViewType.Loading);
                try
                {
                    RPC.Start(ID);
                }
                catch (Exception ex)
                {
                    App.Log.Error("RPC", ex);
                    FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "Could not start RPC");
                    try
                    {
                        RPC.Shutdown();
                    }
                    catch { }
                    EnableElements(true);
                }
            }
            else
            {
                RPC.AFK = false;
                RPC.Shutdown();
                FrameLiveRPC.Content = new ViewRPC(ViewType.Default);
                EnableElements();
            }
        }

        /// <summary>
        /// Enable all elements when rpc is stopped
        /// </summary>
        public static void EnableElements(bool Failed = false)
        {
            App.WD.BtnToggleRPC.BorderBrush = (Brush)Application.Current.Resources["Brush_Ok"];
            App.WD.BtnToggleRPC.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
            App.WD.BtnToggleRPC.Foreground = (Brush)Application.Current.Resources["Brush_Ok"];
            App.WD.BtnAfk.BorderBrush = (Brush)Application.Current.Resources["Brush_Button"];
            App.WD.BtnAfk.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
            App.WD.BtnAfk.Foreground = (Brush)Application.Current.Resources["Brush_Button"];
            if (RPC.Type == "default")
            {
                App.WD.BtnToggleRPC.Content = "Start MultiRPC";
                App.WD.TabCustom.IsEnabled = true;
                App.WD.TooltipCustom.Visibility = Visibility.Hidden;
            }
            else
            {
                App.WD.MenuProfiles.IsEnabled = true;
                App.WD.TabMultiRPC.IsEnabled = true;
                App.WD.TooltipDefault.Visibility = Visibility.Hidden;
                App.WD.BtnToggleRPC.Content = "Start Custom";
                Views.Custom.ProfileDelete.IsEnabled = true;
                Views.Custom.ProfileDelete.ToolTip = "Delete this profile";
                Views.Custom.ProfileEdit.IsEnabled = true;
                Views.Custom.ProfileEdit.ToolTip = "Rename profile";
                Views.Custom.ProfileAdd.IsEnabled = true;
                Views.Custom.ProfileAdd.ToolTip = "Create new profile";
            }
            App.WD.BtnUpdatePresence.IsEnabled = false;
            if (Failed)
                App.WD.TextStatus.Content = "Failed";
            else
                App.WD.TextStatus.Content = "Disconnected";
            Views.Custom.TextClientID.IsEnabled = true;
        }

        /// <summary>
        /// Disable all elements when rpc is started
        /// </summary>
        public static void DisableElements(bool Ready = false)
        {
            App.WD.BtnToggleRPC.BorderBrush = (Brush)Application.Current.Resources["Brush_No"];
            App.WD.BtnToggleRPC.Background = (Brush)Application.Current.Resources["Brush_No"];
            App.WD.BtnToggleRPC.Foreground = SystemColors.ControlBrush;
            App.WD.BtnToggleRPC.Content = "Shutdown";
            Views.Custom.TextClientID.IsEnabled = false;

            if (RPC.Type == "default")
            {
                App.WD.TabCustom.IsEnabled = false;
                App.WD.TooltipCustom.Visibility = Visibility.Visible;
            }
            else
            {
                App.WD.MenuProfiles.IsEnabled = false;
                App.WD.TabMultiRPC.IsEnabled = false;
                App.WD.TooltipDefault.Visibility = Visibility.Visible;
                Views.Custom.ProfileDelete.IsEnabled = false;
                Views.Custom.ProfileDelete.ToolTip = "Disabled! RPC is running";
                Views.Custom.ProfileEdit.IsEnabled = false;
                Views.Custom.ProfileEdit.ToolTip = "Disabled! RPC is running";
                Views.Custom.ProfileAdd.IsEnabled = false;
                Views.Custom.ProfileAdd.ToolTip = "Disabled! RPC is running";
            }
            if (Ready)
                App.WD.TextStatus.Content = "Connected";
            else
                App.WD.TextStatus.Content = "Loading";
        }

        /// <summary>
        /// Update rpc presence
        /// </summary>
        private void BtnUpdatePresence_Click(object sender, RoutedEventArgs e)
        {
            if (RPC.Type == "default")
            {
                string Large = "";
                if (Views.Default.ItemsLarge.SelectedIndex != -1)
                    Large = (Views.Default.ItemsLarge.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                string Small = "";
                if (Views.Default.ItemsSmall.SelectedIndex != -1)
                    Small = (Views.Default.ItemsSmall.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                if (Views.Default.ToggleTime.IsChecked.Value)
                {
                    if (RPC.Presence.Timestamps == null)
                    {
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                        RPC.StartTime = DateTime.UtcNow;
                        RPC.Uptime.Start();
                    }
                }
                else
                {
                    ViewRPC View = FrameLiveRPC.Content as ViewRPC;
                    View.Time.Content = "";
                    RPC.Uptime.Stop();
                    RPC.Presence.Timestamps = null;
                }
                RPC.SetPresence(Views.Default.TextText1.Text, Views.Default.TextText2.Text, Large, Views.Default.TextLarge.Text, Small, Views.Default.TextSmall.Text);
                RPC.Update();
            }
            else
            {
                if (Views.Custom.ToggleTime.IsChecked.Value)
                {
                    if (RPC.Presence.Timestamps == null)
                    {
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                        RPC.StartTime = DateTime.UtcNow;
                        RPC.Uptime.Start();
                    }
                }
                else
                {
                    ViewRPC View = FrameLiveRPC.Content as ViewRPC;
                    View.Time.Content = "";
                    RPC.Uptime.Stop();
                    RPC.Presence.Timestamps = null;
                }
                RPC.SetPresence(Views.Custom.Profile);
                RPC.Update();
            }
        }

        /// <summary>
        /// Toggle rpc auto mode
        /// </summary>
        private void BtnAuto_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Toggle rpc afk mode
        /// </summary>
        private void BtnAfk_Click(object sender, RoutedEventArgs e)
        {
            if (RPC.AFK)
            {
                if (string.IsNullOrEmpty(TextAfk.Text))
                {
                    RPC.AFK = false;
                    FrameLiveRPC.Content = new ViewRPC(ViewType.Default);
                    EnableElements();
                    try
                    {
                        RPC.Shutdown();
                    }
                    catch { }
                }
                else
                {
                    RPC.Presence.Details = TextAfk.Text;
                    TextAfk.Text = "";
                    RPC.Update();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(TextAfk.Text))
                    MessageBox.Show("You need to enter an afk reason");
                else
                {
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        App.Log.Error("App", $"No network connection available");
                        EnableElements();
                        App.WD.FrameLiveRPC.Content = new ViewRPC(ViewType.Error, "No Network Connection");
                        return;
                    }
                    if (ToggleDiscordCheck.IsChecked.Value && !FuncDiscord.CheckDiscordClient())
                        return;
                    RPC.AFK = true;
                    FrameLiveRPC.Content = new ViewRPC(ViewType.Loading);
                    BtnAfk.BorderBrush = (Brush)Application.Current.Resources["Brush_Button"];
                    BtnAfk.Background = (Brush)Application.Current.Resources["Brush_Button"];
                    BtnAfk.Foreground = SystemColors.ControlBrush;
                    if (Views.Custom.TextClientID.IsEnabled)
                        RPC.Shutdown();
                    DisableElements();
                    BtnUpdatePresence.IsEnabled = false;
                    RPC.SetPresence(TextAfk.Text, "", "cat", "Sleepy cat zzzz", "", "");
                    TextAfk.Text = "";
                    if (ToggleAfkTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                    else
                        RPC.Presence.Timestamps = null;
                    RPC.Start(469643793851744257);
                }
            }
        }

        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!App.FormReady)
                return;
            string Text = (Menu.SelectedItem as TabItem).Header.ToString();
            switch (Text.ToLower())
            {
                case "multirpc":
                    {
                        RPC.Type = "default";
                        if (Views.Custom != null && Views.Custom.TextClientID.IsEnabled)
                            BtnToggleRPC.Content = "Start MultiRPC";
                    }
                    break;
                case "custom":
                    {
                        RPC.Type = "custom";
                        CheckProfileMenuWidth();
                        if (Views.Custom != null && Views.Custom.TextClientID.IsEnabled)
                            BtnToggleRPC.Content = "Start Custom";
                    }
                    break;
                case "settings":
                    {
                        DonationInfo.Text = App.Donation;
                    }
                    break;
            }

        }

        private void Links_Clicked(object sender, MouseButtonEventArgs e)
        {
            string Url = "";
            switch ((sender as Image).Name)
            {
                case "LinkWebsite":
                    {
                        Url = "https://blazedev.me";
                    }
                    break;
                case "LinkDownload":
                    {
                        Url = "https://multirpc.blazedev.me";
                    }
                    break;
                case "LinkGithub":
                    {
                        Url = "https://github.com/xXBuilderBXx/MultiRPC";
                    }
                    break;
                case "LinkServer":
                    {
                        MessageBox.Show("Discord Universe is a fun place to hang out with other users, talk about games, anime or development.\n\nWe also have many high quality bots made by me or other popular devs from anime to music to memes so enjoy playing with them.\n\nThis also serves as a support server for many of my projects so dont be afraid to say hi ;)", "Discord Server", MessageBoxButton.OK, MessageBoxImage.Information);
                        Url = App.SupportServer;
                    }
                    break;
                case "LinkDWebsite":
                    {
                        Url = "https://discordapp.com";
                    }
                    break;
                case "LinkDStatus":
                    {
                        Url = "https://status.discordapp.com/";
                    }
                    break;
                case "LinkDGithub":
                    {
                        Url = "https://github.com/RogueException/Discord.Net";
                    }
                    break;
            }
            if (Url != "")
                Process.Start(Url);
        }

        private void ToggleSetting(object sender, RoutedEventArgs e)
        {
            if (!App.FormReady)
                return;
            if (sender is CheckBox checkBox)
                FuncSettings.ToggleSetting(this, checkBox.Tag.ToString());
            else if (sender is ComboBox comboBox)
                FuncSettings.SelectAutoStart(comboBox);
            else
                App.Log.Error("App", "Unknown setting");
        }

        private void BtnTestError_Click(object sender, RoutedEventArgs e)
        {
            ErrorWindow ErrorWindow = new ErrorWindow
            {
                Test = true
            };
            ErrorWindow.ShowDialog();
        }

        private void BtnTestUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow UpdateWindow = new UpdateWindow
            {
                Test = true
            };
            UpdateWindow.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.Crashed && App.FormReady)
            {
                _Data.SaveProfiles();
                App.Config.Save();
            }
            if (Taskbar != null)
                Taskbar.Dispose();
        }

        private void BntDebugMenu_Click(object sender, RoutedEventArgs e)
        {
            BtnDebugSteam.Visibility = Visibility.Hidden;
            TextDebugSteamID.Visibility = Visibility.Hidden;
            TabDebug.Visibility = Visibility.Visible;
        }

        private void BtnDebugStartRPC_Click(object sender, RoutedEventArgs e)
        {
            FrameLiveRPC.Content = new ViewRPC(ViewType.Loading);
            RPC.SetPresence("Testing", "Debug Mode", "debug", "Beep boop", "", "");
            RPC.Start(450894077165043722);
        }

        private void BtnDebugStopRPC_Click(object sender, RoutedEventArgs e)
        {
            RPC.Shutdown();
            FrameLiveRPC.Content = new ViewRPC(ViewType.Default);
            EnableElements();
        }

        private void BtnDebugSteam_Click(object sender, RoutedEventArgs e)
        {
            RPC.Start(450894077165043722, TextDebugSteamID.Text);
        }

        private void Dev_Clicked(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(App.Developer);
        }

        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            SetLimitVisibility(box, Visibility.Visible);
        }

        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            SetLimitVisibility(box, Visibility.Hidden);
        }

        private void SetLimitVisibility(TextBox box, Visibility vis)
        {
            switch (box.Name)
            {
                case "TextAfk":
                    LimitAfkText.Visibility = vis;
                    break;
            }
        }

        private void Default_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Box = sender as TextBox;
            SetLimitNumber(Box);
            if (Box.Text.Length == 25)
                Box.Opacity = 0.80;
            else
                Box.Opacity = 1;
        }

        private void SetLimitNumber(TextBox box)
        {
            double db = 0.50;
            if (box.Text.Length == 25)
                db = 1;
            else if (box.Text.Length > 20)
                db = 0.90;
            else if (box.Text.Length > 15)
                db = 0.80;
            else if (box.Text.Length > 10)
                db = 0.70;
            else if (box.Text.Length > 5)
                db = 0.60;
            switch (box.Name)
            {
                case "TextAfk":
                    LimitAfkText.Content = 25 - box.Text.Length;
                    LimitAfkText.Opacity = db;
                    break;
            }
        }

        private void BtnChangelog_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow UpdateWindow = new UpdateWindow
            {
                Test = true,
                ViewChangelog = true
            };
            UpdateWindow.ShowDialog();
        }

        private void Link_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            img.Opacity = 1;
            img.Width = 60;
            img.Height = 60;
            img.Margin = new Thickness(int.Parse(img.Tag as string), 48, 0, 0);
        }

        private void Link_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            img.Opacity = 0.6;
            img.Width = 50;
            img.Height = 50;
            img.Margin = new Thickness(int.Parse(img.Tag as string) + 5, 53, 0, 0);
        }

        private void LabelPatreonClick_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.patreon.com/builderb");
        }

        private void LabelPaypalClick_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Paypal donations have to be approved by me please contact me on Discord or join the Discord server");
        }

        private void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                IsEnabled = false;
                Version Version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                App.Version = $"{Version.Major}.{Version.Minor}.{Version.Build}";
                App.WD.TextVersion.Content = App.Version;
                UpdateCheckInfo Info = null;
                try
                {
                    Info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);
                }
                catch { }
                if (Info == null || !Info.UpdateAvailable)
                    MessageBox.Show("You are running the latest version");
                else
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile("https://multirpc.blazedev.me/Changelog.txt", App.ConfigFolder + "Changelog.txt");
                        }
                        using (StreamReader reader = new StreamReader(App.ConfigFolder + "Changelog.txt"))
                        {
                            App.Changelog = reader.ReadToEnd();
                        }
                    }
                    catch { }
                    UpdateWindow UpdateWindow = new UpdateWindow
                    {
                        Info = Info
                    };
                    UpdateWindow.ShowDialog();
                    if (App.StartUpdate)
                    {
                        App.WD.FrameLiveRPC.Content = new ViewRPC(ViewType.Update);
                        FuncUpdater.Start();
                    }
                }
                IsEnabled = true;
            }
        }

        public void ToggleMenu()
        {
            CheckProfileMenuWidth();
            if (_Data.Profiles.Keys.Count() == 1)
            {
                MenuProfiles.Visibility = Visibility.Hidden;
                FrameCustomView.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                MenuProfiles.Visibility = Visibility.Visible;
                FrameCustomView.Margin = new Thickness(0, 25, 0, 0);
            }
        }

        public static void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            _Data.Profiles.TryGetValue((sender as Button).Content.ToString(), out CustomProfile profile);
            if (profile == null)
                return;
            _Data.SaveProfiles();
            Views.Custom = new ViewCustom(profile);
            App.WD.FrameCustomView.Content = Views.Custom;
            foreach (Button b in App.WD.MenuProfiles.Items)
            {
                if (b.Content == (sender as Button).Content)
                    b.Background = (Brush)Application.Current.Resources["Brush_Button"];
                else
                    b.Background = new SolidColorBrush(Color.FromRgb(96, 96, 96));
            }
        }
    }
}
