using MultiRPC.GUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MultiRPC
{
    public partial class MainWindow2 : Window
    {
        private Logger Log = new Logger();
        public static MainWindow2 WD;
        public static void SetLiveView(DiscordRPC.Message.PresenceMessage msg)
        {
            WD.View_LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.View_LiveRPC.Content = new ViewRPCControl(msg);
            });
        }

        public static void SetLiveView(ViewType view)
        {
            WD.View_LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.View_LiveRPC.Content = new ViewRPCControl(view);
            });
        }

        public static void SetRPCUser(string user)
        {
            WD.Label_RPCUser.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.Label_RPCUser.Content = user;
            });
        }
        
        public MainWindow2()
        {
            InitializeComponent();
            try
            {
                Process[] Discord = Process.GetProcessesByName("Discord");

                if (Discord.Count() == 0)
                {
                    Process[] DiscordCanary = Process.GetProcessesByName("DiscordCanary");
                    if (DiscordCanary.Count() == 0)
                    {
                        Error("Discord client not found");
                        Environment.Exit(0);
                    }
                    Title = "MultiRPC - Discord Canary";
                }
                else
                    Title = "MultiRPC - Discord";
            }
            catch(Exception ex)
            {
                Error($"Could not find Discord {ex.Message}");
                Environment.Exit(0);
            }
            
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
                    Toggle_AFKTime.IsChecked = true;
                if (RPC.Config.Custom != null)
                {
                    Text_CustomClientID.Text = RPC.Config.Custom.ID.ToString();
                    Text_CustomText1.Text = RPC.Config.Custom.Text1;
                    Text_CustomText2.Text = RPC.Config.Custom.Text2;
                    Text_CustomLargeKey.Text = RPC.Config.Custom.LargeKey;
                    Text_CustomLargeText.Text = RPC.Config.Custom.LargeText;
                    Text_CustomSmallKey.Text = RPC.Config.Custom.SmallKey;
                    Text_CustomSmallText.Text = RPC.Config.Custom.SmallText;
                }
            }
            RPC.LoadingSettings = false;
            WD = this;
            View_DefaultRPC.Content = new ViewRPCControl(ViewType.Default);
            View_LiveRPC.Content = new ViewRPCControl(ViewType.Default);
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
        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            Log.App("Menu Click: " + (sender as Button).Content);
        }

        //[DllImport("User32.dll")]
        //private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        internal void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Run_RpcStart(object sender, RoutedEventArgs e)
        {
            if (IsRPCOn())
                return;
            RPC.Type = Btn_StartRPC.Tag.ToString();
            EnableRun(false);
            ulong ID = 0;
            if (RPC.Type == "default")
            {
                try
                {
                    ID = 450894077165043722;
                    RPC.CheckField(Text_DefaultText1.Text);
                    RPC.CheckField(Text_DefaultText2.Text);
                    string LKey = "";
                    string SKey = "";
                    if (Items_DefaultLarge.SelectedIndex != -1)
                        LKey = (Items_DefaultLarge.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                    if (Items_DefaultSmall.SelectedIndex != -1)
                        SKey = (Items_DefaultSmall.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
                    RPC.SetPresence(Text_DefaultText1.Text, Text_DefaultText2.Text, LKey, Text_DefaultLargeText.Text, SKey, Text_DefaultSmallText.Text);
                    if (Toggle_DefaultTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(start: DateTime.Now);
                }
                catch (Exception ex)
                {
                    Log.App($"{ex}");
                }
            }
            else if (RPC.Type == "custom")
            {
                RPC.CheckField(Text_CustomText1.Text);
                RPC.CheckField(Text_CustomText2.Text);
                if (!ulong.TryParse(Text_CustomClientID.Text, out ID))
                {
                    DisableRun();
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
                    DisableRun();
                    Error("Client ID is invalid");
                    return;
                }
                RPC.SetPresence(this);
                if (Toggle_CustomTime.IsChecked.Value)
                    RPC.Presence.Timestamps = new DiscordRPC.Timestamps(start: DateTime.Now);
            }

            try
            {
                View_LiveRPC.Content = new ViewRPCControl(ViewType.Loading);
            }
            catch { }
            try
            {
                
                RPC.Start(ID);
                RPC.Config.Save(this);
            }
            catch (Exception ex)
            {
                View_LiveRPC.Content = new ViewRPCControl(ViewType.Error, ex.Message);
                DisableRun(true);
                Error($"Could not start RPC, {ex.Message}");
                Log.App($"ERROR {ex}");
                RPC.Shutdown();
            }
        }

        public bool IsRPCOn()
        {
            return !Text_CustomClientID.IsEnabled;
        }

        public void EnableRun(bool Ready)
        {
            if (Ready)
                Label_RPCStatus.Content = "Connected";
            else
                Label_RPCStatus.Content = "Loading";

            Text_CustomClientID.IsEnabled = false;
            Help_Error.Visibility = Visibility.Visible;
            Help_Error.ToolTip = new Button().Content = "RPC is running you cannot change the ID";
            Btn_StartRPC.Background = (Brush)Application.Current.Resources["Brush_Ok"];
            Btn_StartRPC.Foreground = SystemColors.ControlBrush;
            Btn_ShutdownRPC.Foreground = (Brush)Application.Current.Resources["Brush_No"];
            Btn_ShutdownRPC.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
        }

        public void DisableRun(bool Failed = false)
        {
            if (Failed)
                Label_RPCStatus.Content = "Failed";
            else
                 Label_RPCStatus.Content = "Disconnected";
            Text_CustomClientID.IsEnabled = true;
            Help_Error.Visibility = Visibility.Hidden;
            Help_Error.ToolTip = new Button().Content = "Invalid client ID";
            Btn_StartRPC.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
            Btn_StartRPC.Foreground = (Brush)Application.Current.Resources["Brush_Ok"];
            Btn_ShutdownRPC.Foreground = SystemColors.ControlBrush;
            Btn_ShutdownRPC.Background = (Brush)Application.Current.Resources["Brush_No"];
        }

        private void Run_RpcUpdate(object sender, RoutedEventArgs e)
        {
            if (IsRPCOn())
            {
                Error("You cannot update the RPC if its not running");
                return;
            }
            try
            {
                View_LiveRPC.Content = new ViewRPCControl(ViewType.Loading);
            }
            catch { }
            try
            {
                RPC.SetPresence(this);
                RPC.Update();
                RPC.Config.Save(this);
            }
            catch(Exception ex)
            {
                Error($"Could not update RPC, {ex.Message}");
                Log.App($"ERROR {ex}");
            }
        }

        private void Run_RpcShutdown(object sender, RoutedEventArgs e)
        {
            if (!IsRPCOn())
                return;
            RPC.AFK = false;
            View_LiveRPC.Content = new ViewRPCControl(ViewType.Default);
            DisableRun();
            try
            {
                RPC.Shutdown();
            }
            catch { }
        }

        private void UpdateMenu(string Type)
        {
            View_ProgramSettings.Content = new ProgramSettings();
            // WindowsProgramSettings.IsEnabled = false;
            //   WindowsProgramSettings.Type = Type;
            //   WindowsProgramSettings.Text_Priority.Text = Data.Programs[Type].Priority.ToString();
            //   WindowsProgramSettings.Text_Program.Text = Data.Programs[Type].Name;
            //   WindowsProgramSettings.IsEnabled = true;
        }

        private void MenuClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Log.App("Menu Click: " + sender.ToString());
        }

        private void HelpButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image Caller = (sender as Image);
            if (Caller.Opacity == 1)
            {
                Caller.Opacity = 0.7;
                Image_Help.Source = null;
                return;
            }
            Help_ClientID.Opacity = 0.7;
            Help_Text1.Opacity = 0.7;
            Help_Text2.Opacity = 0.7;
            Help_LargeKey.Opacity = 0.7;
            Help_LargeText.Opacity = 0.7;
            Help_SmallKey.Opacity = 0.7;
            Help_SmallText.Opacity = 0.7;
            Uri Url = null;
            switch (Caller.Name)
            {
                case "Help_ClientID":
                    Url = new Uri("https://i.imgur.com/QFO9nnY.png");
                    Help_ClientID.Opacity = 1;
                    break;
                case "Help_Text1":
                    Url = new Uri("https://i.imgur.com/WF0sOBx.png");
                    Help_Text1.Opacity = 1;
                    break;
                case "Help_Text2":
                    Url = new Uri("https://i.imgur.com/loGpAh7.png");
                    Help_Text2.Opacity = 1;
                    break;
                case "Help_LargeKey":
                    Url = new Uri("https://i.imgur.com/UzHaAgw.png");
                    Help_LargeKey.Opacity = 1;
                    break;
                case "Help_LargeText":
                    Url = new Uri("https://i.imgur.com/CH9JmHG.png");
                    Help_LargeText.Opacity = 1;
                    break;
                case "Help_SmallKey":
                    Url = new Uri("https://i.imgur.com/EoyRYhC.png");
                    Help_SmallKey.Opacity = 1;
                    break;
                case "Help_SmallText":
                    Url = new Uri("https://i.imgur.com/9CkGNiB.png");
                    Help_SmallText.Opacity = 1;
                    break;
            }
            if (Url != null)
            {
                ImageSource imgSource = new BitmapImage(Url);
                Image_Help.Source = imgSource;
            }
        }

        private void Custom_ClientID_Changed(object sender, TextChangedEventArgs e)
        {
            if (Help_Error != null)
            {
                if (Text_CustomClientID.Text.Length < 15 || !ulong.TryParse(Text_CustomClientID.Text, out ulong ID))
                    Help_Error.Visibility = Visibility.Visible;
                else
                    Help_Error.Visibility = Visibility.Hidden;
            }
        }

        private void Click_ToggleAuto(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature is not available");
        }

        private void Click_ToggleAFK(object sender, RoutedEventArgs e)
        {
            if (RPC.AFK)
            {
                if (string.IsNullOrEmpty(Text_AFK.Text))
                {
                    RPC.AFK = false;
                    View_LiveRPC.Content = new ViewRPCControl(ViewType.Default);
                    DisableRun();
                    try
                    {
                        RPC.Shutdown();
                    }
                    catch { }
                }
                else
                {
                    RPC.Presence.State = Text_AFK.Text;
                    RPC.Update();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Text_AFK.Text))
                    MessageBox.Show("You need to enter an afk reason");
                else
                {
                    RPC.AFK = true;
                    View_LiveRPC.Content = new ViewRPCControl(ViewType.Loading);
                    if (IsRPCOn())
                        RPC.Shutdown();
                    EnableRun(false);
                    RPC.SetPresence("Away from keyboard", Text_AFK.Text, "cat", "Sleepy cat zzzz", "", "");
                    if (Toggle_AFKTime.IsChecked.Value)
                        RPC.Presence.Timestamps = new DiscordRPC.Timestamps(start: DateTime.Now);
                    else
                        RPC.Presence.Timestamps = null;
                    RPC.Start(469643793851744257);
                }
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
                        (View_DefaultRPC.Content as ViewRPCControl).Text1.Content = Box.Text;
                        break;
                    case "text2":
                        (View_DefaultRPC.Content as ViewRPCControl).Text2.Content = Box.Text;
                        break;
                    case "large":
                        if (string.IsNullOrEmpty(Box.Text))
                            (View_DefaultRPC.Content as ViewRPCControl).LargeImage.ToolTip = null;
                        else
                            (View_DefaultRPC.Content as ViewRPCControl).LargeImage.ToolTip = new Button().Content = Box.Text;
                        break;
                    case "small":
                        if (string.IsNullOrEmpty(Box.Text))
                            (View_DefaultRPC.Content as ViewRPCControl).SmallImage.ToolTip = null;
                        else
                            (View_DefaultRPC.Content as ViewRPCControl).SmallImage.ToolTip = new Button().Content = Box.Text;
                        break;
                }
            }
            if (Box.Text.Length == 25)
                Box.Opacity = 0.80;
            else
                Box.Opacity = 1;
        }

        private void TabList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsRPCOn())
                return;
            try
            {
                string Text = (TabList.SelectedItem as TabItem).Header.ToString();
                if (Text == "MultiRPC")
                {
                    Btn_StartRPC.Content = "Start MultiRPC";
                    Btn_StartRPC.Tag = "default";
                }
                else if (Text == "Custom")
                {
                    Btn_StartRPC.Content = "Start Custom";
                    Btn_StartRPC.Tag = "custom";
                }
            }
            catch { }
        }

        private void Items_Default_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox Box = sender as ComboBox;
            if (Box.SelectedIndex != -1)
            {
                ViewRPCControl View = View_DefaultRPC.Content as ViewRPCControl;
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

        private void Toggle_AFKTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!RPC.LoadingSettings)
            {
                RPC.Config.AFKTime = Toggle_AFKTime.IsChecked.Value;
                RPC.Config.Save();
            }
        }
    }
}
