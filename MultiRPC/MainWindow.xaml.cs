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
    public partial class MainWindow : Window
    {
        private Logger Log = new Logger();
        public static MainWindow WD;
        public static void SetLiveView(DiscordRPC.Message.PresenceMessage msg)
        {
            WD.View_LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.View_LiveRPC.Content = new ViewRPC(msg);
            });
        }

        public static void SetRPCUser(string user)
        {
            WD.Label_RPCUser.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.Label_RPCUser.Content = user;
            });
        }

        public static void SetLiveView(string type, string message = "")
        {
            WD.View_LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.View_LiveRPC.Content = new ViewRPC(type, message, "", "", "","", "");
            });
        }

        private bool Canary = false;
        public MainWindow()
        {
            try
            {
                Process[] Discord = Process.GetProcessesByName("Discord");
                if (Discord.Count() == 0)
                {
                    Canary = true;
                    Process[] DiscordCanary = Process.GetProcessesByName("DiscordCanary");
                    if (DiscordCanary.Count() == 0)
                    {
                        Error("Discord client not found");
                        Environment.Exit(0);
                    }
                }
            }
            catch(Exception ex)
            {
                Error("Could not find Discord");
                Log.App($"ERROR! {ex}");
            }
            InitializeComponent();
            if (Canary)
                Title = "MultiRPC - Discord Canary";
            else
                Title = "MultiRPC - Discord";
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
            WD = this;
            View_LiveRPC.Content = new ViewRPC("MultiRPC", "Thanks for using", "this program", "https://cdn.discordapp.com/app-assets/450894077165043722/450897709013008385.png", "https://cdn.discordapp.com/app-assets/450894077165043722/450897709013008385.png", "Lol", "Hi");
            
            Data.Load();
            foreach (IProgram P in Data.Programs.Values.OrderBy(x => x.Data.Priority).Reverse())
            {
                Button B = new Button
                {
                    Content = P.Name
                };
                B.Click += B_Click;
                if (P.Auto)
                    B.Foreground = SystemColors.HotTrackBrush;
                else
                    B.Foreground = SystemColors.ActiveCaptionTextBrush;
                List_Programs.Items.Add(B);
                Log.Program($"Loaded {P.Name}: {P.Data.Enabled} ({P.Data.Priority})");
            }
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

            EnableRun(false);
            if (!ulong.TryParse(Text_CustomClientID.Text, out ulong ID))
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
            try
            {
                SetLiveView("load");
            }
            catch { }
            try
            {
                RPC.SetPresence(this);
                RPC.Start(ID);
                RPC.Config.Save(this);
            }
            catch (Exception ex)
            {
                DisableRun(true);
                Error($"Could not start RPC, {ex.Message}");
                Log.App($"ERROR {ex}");
                RPC.Shutdown();
            }
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
            if (Text_CustomClientID.IsEnabled)
            {
                Error("You cannot update the RPC if its not running");
                return;
            }
            try
            {
                SetLiveView("load");
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
            View_LiveRPC.Content = new ViewRPC("MultiRPC", "Thanks for using", "this program", "https://cdn.discordapp.com/app-assets/450894077165043722/450897709013008385.png", "https://cdn.discordapp.com/app-assets/450894077165043722/450897709013008385.png", "Lol", "Hi");

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
    }
}
