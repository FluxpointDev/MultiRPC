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

namespace MultiRPC.GUI
{
    public partial class MainWindow : Window
    {
        public static CustomPage CustomPage;
        public MainWindow(Style style)
        {
            InitializeComponent();
            this.StateChanged += MainWindow_StateChanged;
            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2)
            {
                ItemsDefaultLarge.Style = style;
                ItemsDefaultSmall.Style = style;
                ItemsPipe.Style = style;
                ItemsAutoStart.Style = style;
            }
            TabDebug.Visibility = Visibility.Hidden;
            IsEnabled = false;
            if (!Directory.Exists(RPC.ConfigFolder))
                Directory.CreateDirectory(RPC.ConfigFolder);
            string OldFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC.json";
            if (File.Exists(OldFile))
                File.Move(OldFile, RPC.ConfigFile);

            if (File.Exists(RPC.ConfigFile))
            {
                using (StreamReader reader = new StreamReader(RPC.ConfigFile))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    RPC.Config = (Config)serializer.Deserialize(reader, typeof(Config));
                }
                ViewLiveRPC.Content = new ViewRPC(ViewType.Default);
                ViewDefaultRPC.Content = new ViewRPC(ViewType.Default2);
            }
            else
                RPC.Config.Save();
            
            //   foreach (IProgram P in Data.Programs.Values.OrderBy(x => x.Data.Priority).Reverse())
            //  {
            //     Button B = new Button
            //      {
            //          Content = P.Name
            //      };
            //     B.Click += B_Click;
            //     if (P.Auto)
            //         B.Foreground = SystemColors.HotTrackBrush;
            //     else
            //        B.Foreground = SystemColors.ActiveCaptionTextBrush;
            //    List_Programs.Items.Add(B);
            //    Log.Program($"Loaded {P.Name}: {P.Data.Enabled} ({P.Data.Priority})");
            // }

            //new UpdateWindow().Show();
            try
            {
                Process[] Discord = Process.GetProcessesByName("Discord");
                if (Discord.Count() != 0)
                    Title = "MultiRPC - Discord";
                else
                {
                    Process[] DiscordCanary = Process.GetProcessesByName("DiscordCanary");
                    if (DiscordCanary.Count() != 0)
                        Title = "MultiRPC - Discord Canary";
                    else
                    {
                        Process[] DiscordPTB = Process.GetProcessesByName("DiscordPTB");
                        if (DiscordPTB.Count() != 0)
                            Title = "MultiRPC - Discord PTB";
                    }
                }
            }
            catch { }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (Window.WindowState == WindowState.Normal)
                Visibility = Visibility.Hidden;
            WindowState = WindowState.Normal;
        }

        public TaskbarIcon Taskbar;

        /// <summary>
        /// When window loads check for update and configure the config
        /// </summary>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FuncUpdater.Check(this);
            if (!App.StartUpdate)
            {
                Taskbar = new TaskbarIcon
                {
                    IconSource = Icon,
                    Name = "MultiRPC",
                    ToolTipText = "MultiRPC"
                };
                Taskbar.TrayLeftMouseDown += Taskbar_TrayLeftMouseDown;
                FuncDiscord.LoadPipes();
                _Data.Load();
                FormReady = true;
                FuncData.MainData(this);
                FileWatch.Create();
                if (File.Exists(RPC.ProfilesFile))
                {
                    using (StreamReader reader = new StreamReader(RPC.ProfilesFile))
                    {
                        JsonSerializer serializer = new JsonSerializer
                        {
                            Formatting = Formatting.Indented
                        };
                        _Data.Profiles = (Dictionary<string, CustomProfile>)serializer.Deserialize(reader, typeof(Dictionary<string, CustomProfile>));
                        
                    }
                    foreach(CustomProfile p in _Data.Profiles.Values)
                    {
                        if (CustomPage == null)
                        {
                            CustomPage = new CustomPage(p);
                            ViewCustomPage.Content = CustomPage;
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
                    App.WD.ToggleMenu();
                    CheckProfileMenuWidth();
                }
                else
                {
                    CustomProfile profile = new CustomProfile { Name = "Custom" };
                    if (RPC.Config.Custom != null)
                    {
                        profile.ClientID = RPC.Config.Custom.ID.ToString();
                        profile.Text1 = RPC.Config.Custom.Text1;
                        profile.Text2 = RPC.Config.Custom.Text2;
                        profile.LargeKey = RPC.Config.Custom.LargeKey;
                        profile.LargeText = RPC.Config.Custom.LargeText;
                        profile.SmallKey = RPC.Config.Custom.SmallKey;
                        profile.SmallText = RPC.Config.Custom.SmallText;
                        RPC.Config.Custom = null;
                        RPC.Config.Save();
                    }
                    _Data.Profiles.Add("Custom", profile);
                    _Data.SaveProfiles();
                    CustomPage = new CustomPage(profile);
                    ViewCustomPage.Content = CustomPage;
                    (MenuProfiles.Items[0] as Button).Click += ProfileBtn_Click;
                }
                IsEnabled = true;
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

        public static bool FormReady = false;

        /// <summary>
        /// Update rpc view with presence data
        /// </summary>
        /// <param name="msg"></param>
        public static void SetLiveView(DiscordRPC.Message.PresenceMessage msg)
        {
            App.WD.ViewLiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                App.WD.ViewLiveRPC.Content = new ViewRPC(msg);
            });
        }

        /// <summary>
        /// Update roc view with default modes
        /// </summary>
        public static void SetLiveView(ViewType view, string error = "")
        {
            App.WD.ViewLiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                App.WD.ViewLiveRPC.Content = new ViewRPC(view, error);
            });
        }

        /// <summary>
        /// Start/Stop rpc
        /// </summary>
        public void BtnToggleRPC_Click(object sender, RoutedEventArgs e)
        {
            if (MenuProfiles.IsEnabled)
            {
                if (CustomPage == null)
                    return;
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    RPC.Log.Error("App", $"No network connection available");
                    App.WD.ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "No Network Connection!");
                    return;
                }
                if (ToggleDiscordCheck.IsChecked.Value && !FuncDiscord.CheckDiscordClient())
                    return;
                DisableElements();
                ulong ID = 0;
                if (RPC.Type == "default")
                {
                    ID = 450894077165043722;
                    RPC.CheckField(TextDefaultText1.Text);
                    RPC.CheckField(TextDefaultText2.Text);
                    string LKey = "";
                    string SKey = "";
                    if (ItemsDefaultLarge.SelectedIndex != -1)
                        LKey = (ItemsDefaultLarge.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                    if (ItemsDefaultSmall.SelectedIndex != -1)
                        SKey = (ItemsDefaultSmall.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                    RPC.SetPresence(TextDefaultText1.Text, TextDefaultText2.Text, LKey, TextDefaultLarge.Text, SKey, TextDefaultSmall.Text);
                    if (ToggleDefaultTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);

                }
                else if (RPC.Type == "custom")
                {
                    RPC.CheckField(CustomPage.Text1.Text);
                    RPC.CheckField(CustomPage.Text2.Text);
                    CustomPage.TextClientID.Text = CustomPage.TextClientID.Text.Replace(" ", "");
                    if (!ulong.TryParse(CustomPage.TextClientID.Text, out ID))
                    {
                        EnableElements(true);
                        RPC.Log.Error("App", $"Client ID is invalid");
                        ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "Client ID is invalid");
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
                            RPC.Log.Error("API", $"Could not connect to Discord API");
                            ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "Network issue!");
                            EnableElements(true);
                            return;
                        }
                       
                        if (T.StatusCode == HttpStatusCode.BadRequest)
                        {
                            RPC.Log.Error("API", $"Client ID is invalid");
                            ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "Client ID is invalid!");
                            EnableElements(true);
                            return;
                        }
                        if (T.StatusCode != HttpStatusCode.InternalServerError)
                        {
                            string Response = T.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            RPC.Log.Error("API", $"API error {Response}");
                            ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "API Issue!");
                            EnableElements(true);
                            return;
                        }
                    }
                    RPC.SetPresence(CustomPage.Profile);
                    if (CustomPage.ToggleTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                }
                ViewLiveRPC.Content = new ViewRPC(ViewType.Loading);
                try
                {
                    RPC.Start(ID);
                }
                catch (Exception ex)
                {
                    RPC.Log.Error("RPC", ex);
                    ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "Could not start RPC");
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
                ViewLiveRPC.Content = new ViewRPC(ViewType.Default);
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
            App.WD.MenuProfiles.IsEnabled = true;
            App.WD.BtnAfk.BorderBrush = (Brush)Application.Current.Resources["Brush_Button"];
            App.WD.BtnAfk.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
            App.WD.BtnAfk.Foreground = (Brush)Application.Current.Resources["Brush_Button"];
            if (RPC.Type == "default")
                App.WD.BtnToggleRPC.Content = "Start MultiRPC";
            else
                App.WD.BtnToggleRPC.Content = "Start Custom";
            App.WD.TooltipCustom.Visibility = Visibility.Hidden;
            App.WD.TooltipDefault.Visibility = Visibility.Hidden;
            App.WD.TabCustom.IsEnabled = true;
            App.WD.TabMultiRPC.IsEnabled = true;
            App.WD.BtnUpdatePresence.IsEnabled = false;
            if (Failed)
                App.WD.TextStatus.Content = "Failed";
            else
                App.WD.TextStatus.Content = "Disconnected";
            CustomPage.HelpError.Visibility = Visibility.Hidden;
            CustomPage.TextClientID.IsEnabled = true;
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
            CustomPage.TextClientID.IsEnabled = false;
            App.WD.MenuProfiles.IsEnabled = false;
            if (RPC.Type == "default")
            {
                App.WD.TabCustom.IsEnabled = false;
                App.WD.TooltipCustom.Visibility = Visibility.Visible;
            }
            else
            {
                App.WD.TabMultiRPC.IsEnabled = false;
                App.WD.TooltipDefault.Visibility = Visibility.Visible;
            }
            if (Ready)
                App.WD.TextStatus.Content = "Connected";
            else
                App.WD.TextStatus.Content = "Loading";
            CustomPage.HelpError.Visibility = Visibility.Visible;
            CustomPage.HelpError.ToolTip = new Button().Content = "RPC is running";
        }
        
        /// <summary>
        /// Update rpc presence
        /// </summary>
        private void BtnUpdatePresence_Click(object sender, RoutedEventArgs e)
        {
            RPC.Config.Save(this);
            if (RPC.Type == "default")
            {
                string Large = "";
                if (ItemsDefaultLarge.SelectedIndex != -1)
                    Large = (ItemsDefaultLarge.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                string Small = "";
                if (ItemsDefaultSmall.SelectedIndex != -1)
                    Small = (ItemsDefaultSmall.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                if (ToggleDefaultTime.IsChecked.Value)
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
                    ViewRPC View = ViewLiveRPC.Content as ViewRPC;
                    View.Time.Content = "";
                    RPC.Uptime.Stop();
                    RPC.Presence.Timestamps = null;
                }
                RPC.SetPresence(TextDefaultText1.Text, TextDefaultText2.Text, Large, TextDefaultLarge.Text, Small, TextDefaultSmall.Text);
                RPC.Update();
            }
            else
            {
                if (CustomPage.ToggleTime.IsChecked.Value)
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
                    ViewRPC View = ViewLiveRPC.Content as ViewRPC;
                    View.Time.Content = "";
                    RPC.Uptime.Stop();
                    RPC.Presence.Timestamps = null;
                }
                RPC.SetPresence(CustomPage.Profile);
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
                    ViewLiveRPC.Content = new ViewRPC(ViewType.Default);
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
                        RPC.Log.Error("App", $"No network connection available");
                        EnableElements();
                        App.WD.ViewLiveRPC.Content = new ViewRPC(ViewType.Error, "No Network Connection");
                        return;
                    }
                    if (ToggleDiscordCheck.IsChecked.Value && !FuncDiscord.CheckDiscordClient())
                        return;
                    RPC.AFK = true;
                    
                    ViewLiveRPC.Content = new ViewRPC(ViewType.Loading);
                    BtnAfk.BorderBrush = (Brush)Application.Current.Resources["Brush_Button"];
                    BtnAfk.Background = (Brush)Application.Current.Resources["Brush_Button"];
                    BtnAfk.Foreground = SystemColors.ControlBrush;
                    if (MenuProfiles.IsEnabled)
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

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FormReady)
                return;
            ComboBox Box = sender as ComboBox;
            if (Box.SelectedIndex != -1)
            {
                ViewRPC View = ViewDefaultRPC.Content as ViewRPC;
                if (Box.Tag.ToString() == "large")
                {
                    if (Box.SelectedIndex == 0)
                    {
                        View.LargeImage.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        BitmapImage Large = new BitmapImage(new Uri(_Data.MultiRPC_Images[(Box.SelectedItem as ComboBoxItem).Content.ToString()]));
                        Large.DownloadFailed += ViewRPC.Image_FailedLoading;
                        View.LargeImage.Visibility = Visibility.Visible;
                        View.LargeImage.Source = Large;
                    }
                }
                else
                {
                    if (Box.SelectedIndex == 0)
                    {
                        View.SmallBack.Visibility = Visibility.Hidden;
                        View.SmallImage.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        BitmapImage Small = new BitmapImage(new Uri(_Data.MultiRPC_Images[(Box.SelectedItem as ComboBoxItem).Content.ToString()]));

                        Small.DownloadFailed += ViewRPC.Image_FailedLoading;
                        View.SmallBack.Visibility = Visibility.Visible;
                        View.SmallImage.Visibility = Visibility.Visible;
                        View.SmallImage.Fill = new ImageBrush(Small);
                    }
                }
            }

        }
        
        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FormReady)
                return;
            string Text = (Menu.SelectedItem as TabItem).Header.ToString();
            switch (Text.ToLower())
            {
                case "multirpc":
                    {
                        RPC.Type = "default";
                        if (MenuProfiles.IsEnabled)
                            BtnToggleRPC.Content = "Start MultiRPC";
                    }
                    break;
                case "custom":
                    {
                        RPC.Type = "custom";
                        CheckProfileMenuWidth();
                        if (MenuProfiles.IsEnabled)
                            BtnToggleRPC.Content = "Start Custom";
                    }
                    break;
                case "settings":
                    {
                        DonationInfo.Text = App.Donation;
                    }
                    break;
                case "credits":
                    {
                        using (StreamWriter file = File.CreateText(RPC.ConfigFolder + "Temp.json"))
                        {
                            JsonSerializer serializer = new JsonSerializer
                            {
                                Formatting = Formatting.Indented
                            };
                            serializer.Serialize(file, new CreditsList());
                        }

                        if (FuncCredits.CreditsList == null)
                        {
                            FuncCredits.Download();
                            if (File.Exists(RPC.ConfigFolder + "Credits.json"))
                            {
                                try
                                {
                                    using (StreamReader reader = new StreamReader(RPC.ConfigFolder + "Credits.json"))
                                    {
                                        JsonSerializer serializer = new JsonSerializer
                                        {
                                            Formatting = Formatting.Indented
                                        };
                                        FuncCredits.CreditsList = (CreditsList)serializer.Deserialize(reader, typeof(CreditsList));
                                    }
                                }
                                catch { }
                                if (FuncCredits.CreditsList != null)
                                {

                                    ListAdmins.Text = string.Join("\n\n", FuncCredits.CreditsList.Admins);
                                    ListPatreon.Text = string.Join("\n\n", FuncCredits.CreditsList.Patreon);
                                    ListPaypal.Text = string.Join("\n\n", FuncCredits.CreditsList.Paypal);
                                }
                            }
                        }
                    }
                    break;
            }

        }

        private void Default_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!FormReady)
                return;
            TextBox Box = sender as TextBox;
            ViewRPC View = ViewDefaultRPC.Content as ViewRPC;
            switch (Box.Name)
            {
                case "TextDefaultText1":
                    View.Text1.Content = Box.Text;
                    break;
                case "TextDefaultText2":
                    View.Text2.Content = Box.Text;
                    break;
                case "TextDefaultLarge":
                    if (string.IsNullOrEmpty(Box.Text))
                        View.LargeImage.ToolTip = null;
                    else
                        View.LargeImage.ToolTip = new Button().Content = Box.Text;
                    break;
                case "TextDefaultSmall":
                    if (string.IsNullOrEmpty(Box.Text))
                        View.SmallImage.ToolTip = null;
                    else
                        View.SmallImage.ToolTip = new Button().Content = Box.Text;
                    break;
            }
            SetLimitNumber(Box);
            if (Box.Text.Length == 25)
            {

                Box.Opacity = 0.80;
            }
            else
            {
                Box.Opacity = 1;
            }
        }

        private void Links_Clicked(object sender, MouseButtonEventArgs e)
        {
            string Url = "";
            switch((sender as Image).Name)
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
            if (!FormReady)
                return;
            if (sender is CheckBox checkBox)
                FuncSettings.ToggleSetting(this, checkBox.Tag.ToString());
            else if (sender is ComboBox comboBox)
                FuncSettings.SelectAutoStart(comboBox);
            else
                RPC.Log.Error("App", "Unknown setting");
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
            if (App.SettingsLoaded)
                RPC.Config.Save(this);
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
            ViewLiveRPC.Content = new ViewRPC(ViewType.Loading);
            RPC.SetPresence("Testing", "Debug Mode", "debug", "Beep boop", "", "");
            RPC.Start(450894077165043722);
        }

        private void BtnDebugStopRPC_Click(object sender, RoutedEventArgs e)
        {
            RPC.Shutdown();
            ViewLiveRPC.Content = new ViewRPC(ViewType.Default);
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
                case "TextDefaultText1":
                    LimitDefaultText1.Visibility = vis;
                    break;
                case "TextDefaultText2":
                    LimitDefaultText2.Visibility = vis;
                    break;
                case "TextDefaultLarge":
                    LimitDefaultLargeText.Visibility = vis;
                    break;
                case "TextDefaultSmall":
                    LimitDefaultSmallText.Visibility = vis;
                    break;

                case "TextAfk":
                    LimitAfkText.Visibility = vis;
                    break;
            }
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
                case "TextDefaultText1":
                    LimitDefaultText1.Content = 25 - box.Text.Length;
                    LimitDefaultText1.Opacity = db;
                    break;
                case "TextDefaultText2":
                    LimitDefaultText2.Content = 25 - box.Text.Length;
                    LimitDefaultLargeText.Opacity = db;
                    break;
                case "TextDefaultLarge":
                    LimitDefaultLargeText.Content = 25 - box.Text.Length;
                    LimitDefaultLargeText.Opacity = db;
                    break;
                case "TextDefaultSmall":
                    LimitDefaultSmallText.Content = 25 - box.Text.Length;
                    LimitDefaultSmallText.Opacity = db;
                    break;

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
                            client.DownloadFile("https://multirpc.blazedev.me/Changelog.txt", RPC.ConfigFolder + "Changelog.txt");
                        }
                        using (StreamReader reader = new StreamReader(RPC.ConfigFolder + "Changelog.txt"))
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
                        App.WD.ViewLiveRPC.Content = new ViewRPC(ViewType.Update);
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
                ViewCustomPage.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                MenuProfiles.Visibility = Visibility.Visible;
                ViewCustomPage.Margin = new Thickness(0, 25, 0, 0);
            }
        }

        public static void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            _Data.Profiles.TryGetValue((sender as Button).Content.ToString(), out CustomProfile profile);
            if (profile == null)
                return;
            _Data.SaveProfiles();
            CustomPage = new CustomPage(profile);
            App.WD.ViewCustomPage.Content = CustomPage;
            foreach(Button b in App.WD.MenuProfiles.Items)
            {
                if (b.Name == (sender as Button).Name)
                    b.Background = (Brush)Application.Current.Resources["Brush_Button"];
                else
                    b.Background = new SolidColorBrush(Color.FromRgb(96, 96, 96));
            }
        }
    }
}
