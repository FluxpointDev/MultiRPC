using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using Hardcodet.Wpf.TaskbarNotification;
using MultiRPC.GUI.Pages;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TaskbarIcon TaskbarIcon;

        public object ToReturn;
        public long WindowID; //This is so we can know that the window we look for is the window we are looking for

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public MainWindow()
        {
            InitializeComponent();
            var mainPage = new MainPage();
            MinWidth = mainPage.MinWidth;
            MinHeight = mainPage.MinHeight + 30;
            Width = MinWidth;
            Height = MinHeight;
            mainPage.ContentFrame.Navigated += MainPageContentFrame_OnNavigated;
            ContentRendered += MainWindow_ContentRendered;
            ContentFrame.Content = mainPage;
            TaskbarIcon = new TaskbarIcon();
            TaskbarIcon.IconSource = Icon;
            TaskbarIcon.TrayLeftMouseDown += IconOnTrayLeftMouseDown;
            TaskbarIcon.TrayToolTip = new ToolTip(App.Text.HideMultiRPC);
        }

        private void IconOnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Minimized : WindowState.Normal;
            if(App.Config.HideTaskbarIconWhenMin)
                ShowInTaskbar = WindowState == WindowState.Minimized ? false : true;
            TaskbarIcon.TrayToolTip = new ToolTip(WindowState == WindowState.Minimized ? App.Text.ShowMultiRPC : App.Text.HideMultiRPC);
            if (WindowState == WindowState.Normal)
                Activate();
        }

        public static async Task CloseWindow(long WindowID, object Return = null)
        {
            foreach (var window in App.Current.Windows)
            {
                if (window is MainWindow && ((MainWindow)window).WindowID == WindowID)
                {
                    var win = ((MainWindow)window);
                    win.ToReturn = Return;
                    win.Close();
                }
            }
        }

        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (App.Current.MainWindow != this)
                return;

            if (App.Config.DiscordCheck)
            {
                while (MainPage.mainPage.spCheckForDiscord.Visibility == Visibility.Visible)
                    await Task.Delay(760);
                await Task.Delay(250);
            }
            if (App.Config.AutoStart == "MultiRPC")
            {
                MainPage.mainPage.butStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (App.Config.AutoStart == App.Text.Custom)
            {
                MainPage.mainPage.butCustom.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                MainPage.mainPage.butStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        public MainWindow(Page page, bool MinButton = true)
        {
            InitializeComponent();
            MinWidth = page.MinWidth;
            MinHeight = page.MinHeight + 30;
            Width = MinWidth;
            Height = MinHeight;
            ShowInTaskbar = false;
            Title = "MultiRPC - " + page.Title;
            tbTitle.Text = Title;
            ContentFrame.Content = page;

            if (!MinButton)
                butMin.Visibility = Visibility.Collapsed;
        }

        private void RecHandle_OnMouseDown(object sender, MouseButtonEventArgs e)
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
            Close();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowsContent.Margin = new Thickness(7);
            else
                WindowsContent.Margin = new Thickness(0);
        }

        private void Close_OnMouseEnter(object sender, MouseEventArgs e)
        {
            CloseIcon.Fill = Brushes.White;
        }

        private void Close_OnMouseLeave(object sender, MouseEventArgs e)
        {
            CloseIcon.Fill = (Brush)App.Current.Resources["TextColourSCBrush"];
        }

        private void MainPageContentFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.Content != null && App.Current.MainWindow == this)
            {
                bool runCode = true;
                if (RPC.RPCClient != null)
                    if (!RPC.RPCClient.Disposed || RPC.RPCClient.IsInitialized)
                        runCode = false;

                var content = ((Frame)sender).Content;
                if (runCode)
                {
                    if (content is MultiRPCPage)
                        ((MainPage) ContentFrame.Content).butStart.Content = $"{App.Text.Start} MuiltiRPC";
                    else if (content is CustomPage)
                        ((MainPage) ContentFrame.Content).butStart.Content = App.Text.StartCustom;
                }
                else
                {
                    ((MainPage) ContentFrame.Content).butUpdate.IsEnabled = true;
                    if (content is MultiRPCPage && RPC.Type != RPC.RPCType.MultiRPC)
                        ((MainPage)ContentFrame.Content).butUpdate.IsEnabled = false;
                    else if (content is CustomPage && RPC.Type != RPC.RPCType.Custom)
                        ((MainPage)ContentFrame.Content).butUpdate.IsEnabled = false;
                    else if (!(content is CustomPage) && !(content is MultiRPCPage))
                        ((MainPage)ContentFrame.Content).butUpdate.IsEnabled = false;
                }
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            WindowsContent.BorderThickness = new Thickness(1);
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            WindowsContent.BorderThickness = new Thickness(0);
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if(Icon != null && App.Config.HideTaskbarIconWhenMin)
                ShowInTaskbar = WindowState == WindowState.Minimized ? false : true;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            TaskbarIcon?.Dispose();
        }
    }
}
