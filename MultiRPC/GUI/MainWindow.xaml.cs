using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Deployment.Application;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WD = this;
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
                ViewDefaultRPC.Content = new ViewRPCControl(ViewType.Default2);
                if (RPC.Config.MultiRPC != null)
                {
                    TextDefaultText1.Text = RPC.Config.MultiRPC.Text1;
                    TextDefaultText2.Text = RPC.Config.MultiRPC.Text2;
                    TextDefaultLarge.Text = RPC.Config.MultiRPC.LargeText;
                    TextDefaultSmall.Text = RPC.Config.MultiRPC.SmallText;
                }
                if (RPC.Config.Custom != null)
                {
                    TextCustomClientID.Text = RPC.Config.Custom.ID.ToString();
                    TextCustomText1.Text = RPC.Config.Custom.Text1;
                    TextCustomText2.Text = RPC.Config.Custom.Text2;
                    TextCustomLargeKey.Text = RPC.Config.Custom.LargeKey;
                    TextCustomLargeText.Text = RPC.Config.Custom.LargeText;
                    TextCustomSmallKey.Text = RPC.Config.Custom.SmallKey;
                    TextCustomSmallText.Text = RPC.Config.Custom.SmallText;
                }
            }
            ViewLiveRPC.Content = new ViewRPCControl(ViewType.Default);
            Data.Load();
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
            if (RPC.Config.Once)
            {
                try
                {
                    Process[] Discord = Process.GetProcessesByName("MultiRPC");
                    if (Discord.Length == 2)
                        Application.Current.Shutdown();
                }
                catch { }
            }
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
                }
            }
            catch { }
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Version Version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                TextVersion.Content = $"{Version.Major}.{Version.Minor}.{Version.Build}";
                try
                {
                    UpdateCheckInfo CheckInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
                    if (CheckInfo.UpdateAvailable)
                    {
                        IsEnabled = false;
                        Info = CheckInfo;
                    }
                }
                catch
                {
                    Log.Error("Failed to check for updates");
                }
            }
            if (RPC.Config.AFKTime)
                ToggleAfkTime.IsChecked = true;
            if (RPC.Config.Disable.HelpIcons)
                ToggleHelpIcons.IsChecked = true;
            if (RPC.Config.Disable.ProgramsTab)
                ToggleProgramsTab.IsChecked = true;
            if (RPC.Config.AutoStart == "MultiRPC")
                ItemsAutoStart.SelectedIndex = 1;
            else if (RPC.Config.AutoStart == "Custom")
                ItemsAutoStart.SelectedIndex = 2;
        }
        UpdateCheckInfo Info = null;

        private Logger Log = new Logger("App");
        public static MainWindow WD;
        private bool FormReady = false;
        public static void SetLiveView(DiscordRPC.Message.PresenceMessage msg)
        {
            WD.ViewLiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.ViewLiveRPC.Content = new ViewRPCControl(msg);
            });
        }

        public static void SetLiveView(ViewType view, string error = "")
        {
            WD.ViewLiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.ViewLiveRPC.Content = new ViewRPCControl(view, error);
            });
        }

        private void BtnToggleRPC_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRPCOn())
            {
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
                            Log.Error("No Discord client found");
                            ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, "No Discord client");
                            return;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"No Discord client found, {ex.Message}");
                    ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, "No Discord client");
                    return;
                }
                RPC.Type = BtnToggleRPC.Tag.ToString();
                DisableElements(false);
                ulong ID = 0;
                if (RPC.Type == "default")
                {
                    try
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
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to load default fields {ex.Message}");
                        ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, "Invalid default fields");
                    }
                }
                else if (RPC.Type == "custom")
                {
                    RPC.CheckField(TextCustomText1.Text);
                    RPC.CheckField(TextCustomText2.Text);
                    if (!ulong.TryParse(TextCustomClientID.Text, out ID))
                    {
                        EnableElements();
                        ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, "Client ID is invalid");
                        return;
                    }
                    if (!ToggleTokenCheck.IsChecked.Value)
                    {
                        Dictionary<string, string> dict = new Dictionary<string, string>
                        {
                            { "client_id", ID.ToString() }
                        };
                        HttpResponseMessage T = RPC.HttpClient.PostAsync("https://discordapp.com/api/oauth2/token/rpc", new FormUrlEncodedContent(dict)).GetAwaiter().GetResult();
                        if (T.StatusCode.ToString() != "InternalServerError")
                        {
                            EnableElements();
                            ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, "(API) Client ID is invalid");
                            return;
                        }
                    }
                    RPC.SetPresence(this);
                    if (ToggleCustomTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                }
                if (ID == 0)
                {
                    EnableElements();
                    return;
                }
                try
                {
                    ViewLiveRPC.Content = new ViewRPCControl(ViewType.Loading);
                }
                catch { }
                try
                {

                    RPC.Start(ID);
                    RPC.Config.Save(this);
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not start RPC, {ex.Message}");
                    ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, "Could not start RPC");
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
                ViewLiveRPC.Content = new ViewRPCControl(ViewType.Default);
                EnableElements();
            }
        }

        public static void EnableElements(bool Failed = false)
        {
            WD.BtnToggleRPC.BorderBrush = (Brush)Application.Current.Resources["Brush_Ok"];
            WD.BtnToggleRPC.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
            WD.BtnToggleRPC.Foreground = (Brush)Application.Current.Resources["Brush_Ok"];

            WD.BtnAfk.BorderBrush = (Brush)Application.Current.Resources["Brush_Button"];
            WD.BtnAfk.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
            WD.BtnAfk.Foreground = (Brush)Application.Current.Resources["Brush_Button"];
            if (WD.BtnToggleRPC.Tag.ToString() == "default")
                WD.BtnToggleRPC.Content = "Start MultiRPC";
            else
                WD.BtnToggleRPC.Content = "Start Custom";
            WD.TabCustom.IsEnabled = true;
            WD.TabMultiRPC.IsEnabled = true;
            WD.BtnUpdatePresence.IsEnabled = false;
            WD.TextCustomClientID.IsEnabled = true;
            if (Failed)
                WD.TextStatus.Content = "Failed";
            else
                WD.TextStatus.Content = "Disconnected";
            WD.Help_Error.Visibility = Visibility.Hidden;
            WD.Help_Error.ToolTip = new Button().Content = "Invalid client ID";
            WD.TextCustomClientID.IsEnabled = true;
        }

        public static void DisableElements(bool Ready = false)
        {
            WD.BtnToggleRPC.BorderBrush = (Brush)Application.Current.Resources["Brush_No"];
            WD.BtnToggleRPC.Background = (Brush)Application.Current.Resources["Brush_No"];
            WD.BtnToggleRPC.Foreground = SystemColors.ControlBrush;
            WD.BtnToggleRPC.Content = "Shutdown";
            WD.TextCustomClientID.IsEnabled = false;
            if (!RPC.AFK)
                WD.BtnUpdatePresence.IsEnabled = true;
            if (WD.BtnToggleRPC.Tag.ToString() == "default")
                WD.TabCustom.IsEnabled = false;
            else
                WD.TabMultiRPC.IsEnabled = false;
            if (Ready)
                WD.TextStatus.Content = "Connected";
            else
                WD.TextStatus.Content = "Loading";

            WD.Help_Error.Visibility = Visibility.Visible;
            WD.Help_Error.ToolTip = new Button().Content = "RPC is running";
        }
        
        private void BtnUpdatePresence_Click(object sender, RoutedEventArgs e)
        {
            if (BtnToggleRPC.Tag.ToString() == "default")
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
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                }
                else
                    RPC.Presence.Timestamps = null;
                RPC.SetPresence(TextDefaultText1.Text, TextDefaultText2.Text, Large, TextDefaultLarge.Text, Small, TextDefaultSmall.Text);
                RPC.Update();
                RPC.Config.Save(this);
            }
            else
            {
                if (ToggleCustomTime.IsChecked.Value)
                {
                    if (RPC.Presence.Timestamps == null)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                }
                else
                    RPC.Presence.Timestamps = null;
                RPC.SetPresence(this);
                RPC.Update();
            }
        }

        private void BtnAuto_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAfk_Click(object sender, RoutedEventArgs e)
        {
            if (RPC.AFK)
            {
                if (string.IsNullOrEmpty(TextAfk.Text))
                {
                    RPC.AFK = false;
                    ViewLiveRPC.Content = new ViewRPCControl(ViewType.Default);
                    EnableElements();
                    try
                    {
                        RPC.Shutdown();
                    }
                    catch { }
                }
                else
                {
                    RPC.Presence.State = TextAfk.Text;
                    RPC.Update();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(TextAfk.Text))
                    MessageBox.Show("You need to enter an afk reason");
                else
                {
                    WD.BtnAfk.BorderBrush = (Brush)Application.Current.Resources["Brush_Button"];
                    WD.BtnAfk.Background = (Brush)Application.Current.Resources["Brush_Button"];
                    WD.BtnAfk.Foreground = SystemColors.ControlBrush;
                    RPC.AFK = true;
                    ViewLiveRPC.Content = new ViewRPCControl(ViewType.Loading);
                    if (IsRPCOn())
                        RPC.Shutdown();
                    DisableElements();
                    BtnUpdatePresence.IsEnabled = false;
                    RPC.SetPresence(TextAfk.Text, "", "cat", "Sleepy cat zzzz", "", "");
                    if (ToggleAfkTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
                    else
                        RPC.Presence.Timestamps = null;
                    RPC.Start(469643793851744257);
                }
            }
        }

        public bool IsRPCOn()
        {
            return !TextCustomClientID.IsEnabled;
        }

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FormReady)
                return;
            ComboBox Box = sender as ComboBox;
            if (Box.SelectedIndex != -1)
            {
                ViewRPCControl View = ViewDefaultRPC.Content as ViewRPCControl;
                if (Box.Tag.ToString() == "large")
                {
                    if (Box.SelectedIndex == 0)
                        View.LargeImage.Visibility = Visibility.Hidden;
                    else
                    {
                        BitmapImage Large = new BitmapImage(new Uri(Data.MultiRPC_Images[(Box.SelectedItem as ComboBoxItem).Content.ToString()]));
                        Large.DownloadFailed += ViewRPCControl.Image_FailedLoading;
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
                        BitmapImage Small = new BitmapImage(new Uri(Data.MultiRPC_Images[(Box.SelectedItem as ComboBoxItem).Content.ToString()]));
                        Small.DownloadFailed += ViewRPCControl.Image_FailedLoading;
                        View.SmallBack.Visibility = Visibility.Visible;
                        View.SmallImage.Visibility = Visibility.Visible;
                        View.SmallImage.Fill = new ImageBrush(Small);
                    }
                }
            }

        }

        private void ToggleAfkTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!FormReady)
                return;
            RPC.Config.AFKTime = !RPC.Config.AFKTime;
                RPC.Config.Save();
        }

        private void TextField_CheckLimit(object sender, TextChangedEventArgs e)
        {
            TextBox Box = (TextBox)sender;
            if (Box.Tag != null)
            {
                switch (Box.Tag.ToString())
                {
                    case "text1":
                        (ViewDefaultRPC.Content as ViewRPCControl).Text1.Content = Box.Text;
                        break;
                    case "text2":
                        (ViewDefaultRPC.Content as ViewRPCControl).Text2.Content = Box.Text;
                        break;
                    case "large":
                        if (string.IsNullOrEmpty(Box.Text))
                            (ViewDefaultRPC.Content as ViewRPCControl).LargeImage.ToolTip = null;
                        else
                            (ViewDefaultRPC.Content as ViewRPCControl).LargeImage.ToolTip = new Button().Content = Box.Text;
                        break;
                    case "small":
                        if (string.IsNullOrEmpty(Box.Text))
                            (ViewDefaultRPC.Content as ViewRPCControl).SmallImage.ToolTip = null;
                        else
                            (ViewDefaultRPC.Content as ViewRPCControl).SmallImage.ToolTip = new Button().Content = Box.Text;
                        break;
                }
            }
            if (Box.Text.Length == 25)
                Box.Opacity = 0.80;
            else
                Box.Opacity = 1;
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
                        if (!IsRPCOn())
                        {
                            BtnToggleRPC.Content = "Start MultiRPC";
                            BtnToggleRPC.Tag = "default";
                        }
                    }
                    break;
                case "custom":
                    {
                        if (!IsRPCOn())
                        {
                            BtnToggleRPC.Content = "Start Custom";
                            BtnToggleRPC.Tag = "custom";
                        }
                    }
                    break;
                case "settings":
                    {
                       
                    }
                    break;
            }

        }

        private void TextCustomClientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!FormReady)
                return;
            if (TextCustomClientID.Text.Length < 15 || !ulong.TryParse(TextCustomClientID.Text, out ulong ID))
                Help_Error.Visibility = Visibility.Visible;
            else
                Help_Error.Visibility = Visibility.Hidden;

        }

        private void HelpButton_Click(object sender, MouseButtonEventArgs e)
        {
            Image Caller = (sender as Image);
            if (Caller.Opacity == 1)
            {
                Caller.Opacity = 0.7;
                ImageHelp.Source = null;
                return;
            }
            HelpClientID.Opacity = 0.7;
            HelpText1.Opacity = 0.7;
            HelpText2.Opacity = 0.7;
            HelpLargeKey.Opacity = 0.7;
            HelpLargeText.Opacity = 0.7;
            HelpSmallKey.Opacity = 0.7;
            HelpSmallText.Opacity = 0.7;
            Uri Url = null;
            switch (Caller.Name)
            {
                case "HelpClientID":
                    Url = new Uri("https://i.imgur.com/QFO9nnY.png");
                    HelpClientID.Opacity = 1;
                    break;
                case "HelpText1":
                    Url = new Uri("https://i.imgur.com/WF0sOBx.png");
                    HelpText1.Opacity = 1;
                    break;
                case "HelpText2":
                    Url = new Uri("https://i.imgur.com/loGpAh7.png");
                    HelpText2.Opacity = 1;
                    break;
                case "HelpLargeKey":
                    Url = new Uri("https://i.imgur.com/UzHaAgw.png");
                    HelpLargeKey.Opacity = 1;
                    break;
                case "HelpLargeText":
                    Url = new Uri("https://i.imgur.com/CH9JmHG.png");
                    HelpLargeText.Opacity = 1;
                    break;
                case "HelpSmallKey":
                    Url = new Uri("https://i.imgur.com/EoyRYhC.png");
                    HelpSmallKey.Opacity = 1;
                    break;
                case "HelpSmallText":
                    Url = new Uri("https://i.imgur.com/9CkGNiB.png");
                    HelpSmallText.Opacity = 1;
                    break;
            }
            if (Url != null)
            {
                ImageSource imgSource = new BitmapImage(Url);
                ImageHelp.Source = imgSource;
            }
        }

        private void Default_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!FormReady)
                return;
            TextBox Box = sender as TextBox;
            ViewRPCControl View = ViewDefaultRPC.Content as ViewRPCControl;
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
                        Url = "https://discord.gg/WJTYdNb";
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

        #region Updater
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            FormReady = true;
            if (Info != null)
            {
                (ViewLiveRPC.Content as ViewRPCControl).Text1.Content = "Downloading Update";
                (ViewLiveRPC.Content as ViewRPCControl).Text2.Content = "";
                MessageBox.Show("An update is available, downloading update!");
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeployment_UpdateCompleted;
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += CurrentDeployment_UpdateProgressChanged;
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            FormIsReady();
        }

        private void CurrentDeployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            (ViewLiveRPC.Content as ViewRPCControl).Text2.Content = $"{e.ProgressPercentage}%/100%";
        }

        private void CurrentDeployment_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/MultiRPC.appref-ms");
                Application.Current.Shutdown();
            }
            else
                Log.Error($"Failed to update, {e.Error.Message}");
        }
        #endregion

        public void FormIsReady()
        {
            if (RPC.Config.MultiRPC != null)
            {
                ViewRPCControl View = ViewDefaultRPC.Content as ViewRPCControl;
                View.Text1.Content = RPC.Config.MultiRPC.Text1;
                View.Text2.Content = RPC.Config.MultiRPC.Text2;
                if (RPC.Config.MultiRPC != null && RPC.Config.MultiRPC.LargeKey != -1)
                    ItemsDefaultLarge.SelectedItem = ItemsDefaultLarge.Items[RPC.Config.MultiRPC.LargeKey];
                else
                    ItemsDefaultLarge.SelectedItem = ItemsDefaultLarge.Items[1];
                if (RPC.Config.MultiRPC.SmallKey != -1)
                    ItemsDefaultLarge.SelectedItem = ItemsDefaultLarge.Items[RPC.Config.MultiRPC.SmallKey];
                if (!string.IsNullOrEmpty(RPC.Config.MultiRPC.LargeText))
                    View.LargeImage.ToolTip = new Button().Content = RPC.Config.MultiRPC.LargeText;
                if (!string.IsNullOrEmpty(RPC.Config.MultiRPC.SmallText))
                    View.SmallImage.ToolTip = new Button().Content = RPC.Config.MultiRPC.SmallText;
            }
            if (RPC.Config.Disable.HelpIcons)
                DisableHelpIcons();
            if (RPC.Config.Disable.MultiProfiles)
                DisableMulti();
            if (RPC.Config.Disable.ProgramsTab)
            {
                TabPrograms.Width = 0;
                TabPrograms.Visibility = Visibility.Hidden;
            }
            if (RPC.Config.AutoStart == "MultiRPC")
            {
                TabCustom.IsEnabled = false;
                Menu.SelectedIndex = 0;
                BtnToggleRPC_Click(null, null);
            }
            else if (RPC.Config.AutoStart == "Custom")
            {
                TabMultiRPC.IsEnabled = false;
                Menu.SelectedIndex = 1;
                BtnToggleRPC_Click(null, null);
            }
        }

        #region Settings Toggles
        private void ToggleProgramsTab_Checked(object sender, RoutedEventArgs e)
        {
            if (!FormReady)
                return;
            if (ToggleProgramsTab.IsChecked.Value)
            {
                TabPrograms.Width = 0;
                TabPrograms.Visibility = Visibility.Hidden;
            }
            else
            {
                TabPrograms.Width = 67;
                TabPrograms.Visibility = Visibility.Visible;
            }
            RPC.Config.Disable.ProgramsTab = !RPC.Config.Disable.ProgramsTab;
            RPC.Config.Save();
        }

        private void ToggleHelpIcons_Checked(object sender, RoutedEventArgs e)
        {
            if (!FormReady)
                return;
            if (ToggleHelpIcons.IsChecked.Value)
                DisableHelpIcons();
            else
                EnableHelpIcons();
            RPC.Config.Disable.HelpIcons = !RPC.Config.Disable.HelpIcons;
            RPC.Config.Save();
        }

        public void EnableHelpIcons()
        {
            HelpClientID.Visibility = Visibility.Visible;
            HelpText1.Visibility = Visibility.Visible;
            HelpText2.Visibility = Visibility.Visible;
            HelpLargeKey.Visibility = Visibility.Visible;
            HelpLargeText.Visibility = Visibility.Visible;
            HelpSmallKey.Visibility = Visibility.Visible;
            HelpSmallText.Visibility = Visibility.Visible;
        }

        public void DisableHelpIcons()
        {
            HelpClientID.Visibility = Visibility.Hidden;
            HelpText1.Visibility = Visibility.Hidden;
            HelpText2.Visibility = Visibility.Hidden;
            HelpLargeKey.Visibility = Visibility.Hidden;
            HelpLargeText.Visibility = Visibility.Hidden;
            HelpSmallKey.Visibility = Visibility.Hidden;
            HelpSmallText.Visibility = Visibility.Hidden;
        }
        #endregion

        private void ItemsAutoStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormReady)
            {
                RPC.Config.AutoStart = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString();
                RPC.Config.Save();
            }
        }

#region ToggleProfiles
        private void ToggleMultiProfiles_Checked(object sender, RoutedEventArgs e)
        {
            if (!FormReady)
                return;
            if (ToggleMultiProfiles.IsChecked.Value)
                DisableMulti();
            else
                EnableMulti();
            RPC.Config.Disable.MultiProfiles = !RPC.Config.Disable.MultiProfiles;
            RPC.Config.Save();
        }

        public void EnableMulti()
        {
            MenuCustomProfiles.Visibility = Visibility.Visible;
            BtnShareProfile.Visibility = Visibility.Visible;
            BtnAddProfile.Visibility = Visibility.Visible;
            BtnDeleteProfile.Visibility = Visibility.Visible;
        }

        public void DisableMulti()
        {
            MenuCustomProfiles.Visibility = Visibility.Hidden;
            BtnShareProfile.Visibility = Visibility.Hidden;
            BtnAddProfile.Visibility = Visibility.Hidden;
            BtnDeleteProfile.Visibility = Visibility.Hidden;
        }
        #endregion
    }
}
