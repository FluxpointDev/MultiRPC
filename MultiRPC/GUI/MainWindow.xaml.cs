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
                if (RPC.Config.AFKTime)
                    ToggleAfkTime.IsChecked = true;
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
            RPC.LoadingSettings = false;
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
        }

        private Logger Log = new Logger();
        public static MainWindow WD;
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
                            Error($"Could not find Discord client");
                            return;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Error($"Could not find Discord client");
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
                        Log.App($"{ex}");
                    }
                }
                else if (RPC.Type == "custom")
                {
                    RPC.CheckField(TextCustomText1.Text);
                    RPC.CheckField(TextCustomText2.Text);
                    if (!ulong.TryParse(TextCustomClientID.Text, out ID))
                    {
                        EnableElements();
                        Error("Client ID is invalid");
                        return;
                    }
                    Dictionary<string, string> dict = new Dictionary<string, string>
               {
                    { "client_id", ID.ToString() }
               };
                    HttpResponseMessage T = RPC.HttpClient.PostAsync("https://discordapp.com/api/oauth2/token/rpc", new FormUrlEncodedContent(dict)).GetAwaiter().GetResult();
                    if (T.StatusCode.ToString() != "InternalServerError")
                    {
                        EnableElements();
                        Error("Client ID is invalid");
                        return;
                    }
                    RPC.SetPresence(this);
                    if (ToggleCustomTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(DateTime.UtcNow);
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
                    ViewLiveRPC.Content = new ViewRPCControl(ViewType.Error, ex.Message);
                    EnableElements(true);
                    Error($"Could not start RPC, {ex.Message}");
                    Log.App($"ERROR {ex}");
                    RPC.Shutdown();
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

        internal void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    RPC.SetPresence("Away from keyboard", TextAfk.Text, "cat", "Sleepy cat zzzz", "", "");
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
            if (!RPC.LoadingSettings)
            {
                RPC.Config.AFKTime = !RPC.Config.AFKTime;
                RPC.Config.Save();
            }
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
            if (!IsRPCOn())
            {
                string Text = (Menu.SelectedItem as TabItem).Header.ToString();
                if (Text == "MultiRPC")
                {
                    BtnToggleRPC.Content = "Start MultiRPC";
                    BtnToggleRPC.Tag = "default";
                }
                else if (Text == "Custom")
                {
                    BtnToggleRPC.Content = "Start Custom";
                    BtnToggleRPC.Tag = "custom";
                }
            }
        }

        private void TextCustomClientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RPC.LoadingSettings)
            {
                if (TextCustomClientID.Text.Length < 15 || !ulong.TryParse(TextCustomClientID.Text, out ulong ID))
                    Help_Error.Visibility = Visibility.Visible;
                else
                    Help_Error.Visibility = Visibility.Hidden;
            }
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

        private void ItemsDefaultLarge_Loaded(object sender, RoutedEventArgs e)
        {
            if (RPC.Config.MultiRPC != null && RPC.Config.MultiRPC.LargeKey != -1)
                ItemsDefaultLarge.SelectedItem = ItemsDefaultLarge.Items[RPC.Config.MultiRPC.LargeKey];
            else
                ItemsDefaultLarge.SelectedItem = ItemsDefaultLarge.Items[1];
        }

        private void ItemsDefaultSmall_Loaded(object sender, RoutedEventArgs e)
        {
            if (RPC.Config.MultiRPC != null)
            {
                if (RPC.Config.MultiRPC.SmallKey != -1)
                    ItemsDefaultLarge.SelectedItem = ItemsDefaultLarge.Items[RPC.Config.MultiRPC.SmallKey];
                (ViewDefaultRPC.Content as ViewRPCControl).Text1.Content = RPC.Config.MultiRPC.Text1;
                (ViewDefaultRPC.Content as ViewRPCControl).Text2.Content = RPC.Config.MultiRPC.Text2;

            }
        }

        private void Default_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RPC.LoadingSettings)
            {
                TextBox Box = sender as TextBox;
                ViewRPCControl View = ViewDefaultRPC.Content as ViewRPCControl;
                switch(Box.Name)
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
        }
    }
}
