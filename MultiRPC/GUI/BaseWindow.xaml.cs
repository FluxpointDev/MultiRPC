using MultiRPC.Functions;
using MultiRPC.GUI.Pages;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for BaseWindow.xaml
    /// </summary>
    public partial class BaseWindow : Window
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        
        public BaseWindow()
        {
            InitializeComponent();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Version Version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                App.Version = $"{Version.Major}.{Version.Minor}.{Version.Build}";
            }
            LabelMultiRPC.Content = $"MultiRPC v{App.Version}";
            LabelMadeBy.Content = "Made By: " + App.Developer;
            this.Loaded += Start_Loaded;
        }

        public void Error(string text)
        {
            LabelLoading.Content = text;
            ImageError.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/ExitIcon.png", UriKind.Absolute));

        }

        private void Start_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            UpdateCheckInfo Info = null;
            if (ApplicationDeployment.IsNetworkDeployed)
                Info = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);
            if (Info != null && Info.UpdateAvailable)
            {
                LabelLoading.Content = $"Update {Info.AvailableVersion.Major}.{Info.AvailableVersion.Minor}.{Info.AvailableVersion.Build} available";
                ImageError.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/DownloadIcon.png", UriKind.Absolute));
                FrameMain.Height = 460;
                FrameMain.Margin = new Thickness(0, 0, 0, 0);
                FrameMain.Content = new UpdatePage();
            }
            else
                LoadMain();
        }

        public void LoadMain()
        {
            try
            {
                Process[] Discord = Process.GetProcessesByName("Discord");
                if (Discord.Count() != 0)
                    LabelTitle.Text = "MultiRPC - Discord";
                else
                {
                    Process[] DiscordCanary = Process.GetProcessesByName("DiscordCanary");
                    if (DiscordCanary.Count() != 0)
                        LabelTitle.Text = "MultiRPC - Discord Canary";
                    else
                    {
                        Process[] DiscordPTB = Process.GetProcessesByName("DiscordPTB");
                        if (DiscordPTB.Count() != 0)
                            LabelTitle.Text = "MultiRPC - Discord PTB";
                    }
                }
            }
            catch { }
            if (LabelTitle.Text == "MultiRPC")
                Error("Could not find any Discord client");
            else
            {
                
                App.WD = new MainPage(this);
                FrameMain.ContentRendered += FrameMain_ContentRendered;
                FrameMain.Content = App.WD;
            }
        }

        private void FrameMain_ContentRendered(object sender, EventArgs e)
        {
            FrameMain.Height = 460;
            FrameMain.Margin = new Thickness(0, 0, 0, 0);
            LabelMultiRPC.Visibility = Visibility.Hidden;
            LabelLoading.Visibility = Visibility.Hidden;
            LabelMadeBy.Visibility = Visibility.Hidden;
            TextMadeBy.Visibility = Visibility.Hidden;
            TextDiscordLink.Visibility = Visibility.Hidden;
            LabelDiscord.Visibility = Visibility.Hidden;
            ImageMultiRPC.Visibility = Visibility.Hidden;
            FrameMain.Visibility = Visibility.Visible;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                ReleaseCapture();
                SendMessage(hwnd, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                hwnd = new IntPtr(0);
            }
        }

        private void Min_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TextDiscordLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(App.SupportServer);
        }
    }
}
