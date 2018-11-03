using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void SetLiveView(string test)
        {
            WD.View_LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.View_LiveRPC.Content = new ViewRPC(test, "", "", "", "","", "");
            });
        }

        private bool Canary = false;
        public MainWindow()
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
            InitializeComponent();
            if (Canary)
                Title = "MultiRPC - Discord Canary";
            else
                Title = "MultiRPC - Discord";
            WD = this;
            Tab_Theme.Visibility = Visibility.Hidden;
            View_LiveRPC.Content = new ViewRPC("MultiRPC", "Thanks for using", "this program", "https://cdn.discordapp.com/app-assets/450894077165043722/450897709013008385.png", "https://cdn.discordapp.com/app-assets/450894077165043722/450897709013008385.png", "Lol", "Hi");
            View_CustomRPC.Content = new ViewRPC("App name", "Text1", "Text2", "", "", "", "");
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

        [DllImport("User32.dll")]
        private static extern IntPtr FindWindow(string strClassName, string strWindowName);

        internal void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Run_RpcStart(object sender, RoutedEventArgs e)
        {
            EnableRun();
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
            SetLiveView("loadthispls");
            RPC.SetPresence(this);
            RPC.Start(ID);
        }

        public void EnableRun()
        {
            Text_CustomClientID.IsEnabled = false;
            Help_Error.Visibility = Visibility.Visible;
            Help_Error.ToolTip = new Button().Content = "RPC is running you cannot change the ID";
            Btn_StartRPC.Background = (Brush)Application.Current.Resources["Brush_Ok"];
            Btn_StartRPC.Foreground = SystemColors.ControlBrush;
            Btn_ShutdownRPC.Foreground = (Brush)Application.Current.Resources["Brush_No"];
            Btn_ShutdownRPC.Background = (Brush)Application.Current.Resources["Brush_TabBackground"];
        }

        public void DisableRun()
        {
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
            SetLiveView("loadthispls");
            RPC.SetPresence(this);
            RPC.Update();
            //Log.App(RPC.CheckPort().ToString());
            //RPC.upda();
        }

        private void Run_RpcShutdown(object sender, RoutedEventArgs e)
        {
            DisableRun();
            RPC.Shutdown();
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

        private void Run_ShowCustomRpc(object sender, RoutedEventArgs e)
        {
            Log.App("Loading custom RPC");
            View_CustomRPC.Content = new ViewRPC("", Text_CustomText1.Text, Text_CustomText2.Text, "https://cdn.discordapp.com/app-assets/450894077165043722/450894466358968331.png", "https://cdn.discordapp.com/app-assets/450894077165043722/450894923743363073.png", Text_CustomLargeText.Text, Text_CustomSmallText.Text);
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
