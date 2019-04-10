using System;
using System.Linq;
using System.Windows;
using MultiRPC.GUI.Pages;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using MultiRPC.JsonClasses;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using Hardcodet.Wpf.TaskbarNotification;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TaskbarIcon TaskbarIcon;
        private Storyboard CloseStoryboard;
        private Storyboard OpenStoryboard = new Storyboard();

        public object ToReturn;
        public long WindowID; //This is so we can know that the window we look for is the window we are looking for

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private DateTime TimeWindowWasDeactivated;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public MainWindow()
        {
            InitializeComponent();
            ContentRendered += MainWindow_ContentRendered;
            if (this != App.Current.MainWindow)
                return;

            var mainPage = new MainPage();
            StartLogic(mainPage);
            MakeJumpList();
            mainPage.ContentFrame.Navigated += MainPageContentFrame_OnNavigated;

            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                TaskbarItemInfo = new TaskbarItemInfo();
                TaskbarItemInfo.Description = "MultiRPC";
                TaskbarItemInfo.ThumbnailClipMargin = new Thickness(471, 41, 9, 420);
            }

            TaskbarIcon = new TaskbarIcon();
            TaskbarIcon.IconSource = Icon;
            TaskbarIcon.TrayLeftMouseDown += IconOnTrayLeftMouseDown;
            TaskbarIcon.TrayToolTip = new ToolTip(App.Text.HideMultiRPC);
        }

        public MainWindow(Page page, bool minButton = true)
        {
            InitializeComponent();
            ContentRendered += MainWindow_ContentRendered;
            StartLogic(page);
            ShowInTaskbar = false;
            Title = "MultiRPC - " + page.Title;
            tblTitle.Text = Title;

            if (!minButton)
                btnMin.Visibility = Visibility.Collapsed;
        }

        private void MakeWinAnimation(Storyboard storyboard, double from = 1, double to = 0, bool addEventHandler = true, object parameter = null, Duration lengthToRun = new Duration())
        {
            if (!lengthToRun.HasTimeSpan)
                lengthToRun = new Duration(TimeSpan.FromSeconds(0.4));

            if (parameter == null)
                parameter = Window.OpacityProperty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = "win" + DateTime.Now.Ticks;
                RegisterName(this.Name, this);
            }

            DoubleAnimation winOpacityAnimation = new DoubleAnimation();
            winOpacityAnimation.From = from;
            winOpacityAnimation.To = to;
            winOpacityAnimation.Duration = lengthToRun;
            winOpacityAnimation.EasingFunction = new BackEase();

            storyboard.Children.Add(winOpacityAnimation);
            Storyboard.SetTargetName(winOpacityAnimation, Name);
            Storyboard.SetTargetProperty(winOpacityAnimation, new PropertyPath(parameter));
            if(addEventHandler)
                storyboard.Completed += OpenCloseStoryboard_Completed;
        }

        private void StartLogic(Page page)
        {
            if (page.MinWidth > Width)
            {
                MinWidth = page.MinWidth;
                Width = MinWidth;
            }

            if (page.MinHeight + 30 > Height)
            {
                MinHeight = page.MinHeight + 30;
                Height = MinHeight;
            }

            MaxHeight = page.MaxHeight;
            MaxWidth = page.MaxWidth; 
            ContentFrame.Content = page;
        }

        private void IconOnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            var timeSpan = DateTime.Now.Subtract(TimeWindowWasDeactivated);
            if (timeSpan.TotalSeconds < 1 || WindowState == WindowState.Minimized)
                WindowState = WindowState == WindowState.Normal ? WindowState.Minimized : WindowState.Normal;

            if (WindowState == WindowState.Normal)
                Activate();
        }

        public static async Task CloseWindow(long WindowID, object Return = null)
        {
            foreach (var window in App.Current.Windows)
            {
                if (window is MainWindow mainWindow && ((MainWindow)window).WindowID == WindowID)
                {
                    mainWindow.ToReturn = Return;
                    mainWindow.ShowInTaskbar = false;
                    mainWindow.CloseStoryboard = new Storyboard();
                    mainWindow.MakeWinAnimation(mainWindow.CloseStoryboard);
                    mainWindow.CloseStoryboard.Begin(mainWindow);
                    break;
                }
            }
        }

        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (!File.Exists(App.Config.ActiveTheme))
            {
                App.Config.ActiveTheme = "Assets/Themes/DarkTheme.xaml";
                App.Config.Save();
            }

            await ThemeEditorPage.UpdateGlobalUI();
            MakeWinAnimation(OpenStoryboard, 0, 1);
            OpenStoryboard.Begin(this);
            if (App.Current.MainWindow != this)
                return;

            if (App.Config.DiscordCheck)
            {
                while (MainPage.mainPage.gridCheckForDiscord.Visibility == Visibility.Visible)
                    await Task.Delay(760);
                await Task.Delay(250);
            }
            if (App.Config.AutoStart == "MultiRPC")
            {
                MainPage.mainPage.btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (App.Config.AutoStart == App.Text.Custom)
            {
                MainPage.mainPage.btnCustom.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                MainPage.mainPage.btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        public static async Task MakeJumpList()
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                JumpList jumpList = new JumpList();

                for (int i = 0; i < 10; i++)
                {
                    if (i > CustomPage.customPage.Profiles.Count - 1)
                        break;

                    //Configure a new JumpTask
                    JumpTask jumpTask = new JumpTask
                    {
                        // Set the JumpTask properties.
                        ApplicationPath = FileLocations.MultiRPCStartLink,
                        Arguments = $"-custom \"{CustomPage.customPage.Profiles.ElementAt(i).Key}\"",
                        IconResourcePath = FileLocations.MultiRPCStartLink,
                        Title = CustomPage.customPage.Profiles.ElementAt(i).Key,
                        Description = $"{App.Text.Load} '{CustomPage.customPage.Profiles.ElementAt(i).Key}'",
                        CustomCategory = App.Text.CustomProfiles
                    };
                    jumpList.JumpItems.Add(jumpTask);

                }

                JumpList.SetJumpList(App.Current, jumpList);
            }
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
            ShowInTaskbar = false;
            CloseStoryboard = new Storyboard();
            MakeWinAnimation(CloseStoryboard);
            CloseStoryboard.Begin(this);
        }

        private void OpenCloseStoryboard_Completed(object sender, EventArgs e)
        {
            if(CloseStoryboard != null)
                Close();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowsContent.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
        }

        private void Close_OnMouseEnter(object sender, MouseEventArgs e)
        {
            plgCloseIcon.Fill = Brushes.White;
        }

        private void Close_OnMouseLeave(object sender, MouseEventArgs e)
        {
            plgCloseIcon.Fill = (Brush)App.Current.Resources["TextColourSCBrush"];
        }

        private void MainPageContentFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.Content != null && App.Current.MainWindow == this)
            {
                bool runCode = true;
                if (RPC.RPCClient != null)
                    if (!RPC.RPCClient.Disposed && RPC.RPCClient.IsInitialized)
                        runCode = false;

                var content = ((Frame)sender).Content;
                if (runCode)
                {
                    if (content is MultiRPCPage)
                        ((MainPage) ContentFrame.Content).btnStart.Content = $"{App.Text.Start} MuiltiRPC";
                    else if (content is CustomPage)
                        ((MainPage) ContentFrame.Content).btnStart.Content = App.Text.StartCustom;
                }
                else
                {
                    ((MainPage) ContentFrame.Content).btnUpdate.IsEnabled = true;
                    if (content is MultiRPCPage && RPC.Type != RPC.RPCType.MultiRPC)
                    {
                        ((MainPage)ContentFrame.Content).btnUpdate.IsEnabled = false;
                    }
                    else if (content is CustomPage && RPC.Type != RPC.RPCType.Custom)
                    {
                        ((MainPage)ContentFrame.Content).btnUpdate.IsEnabled = false;
                    }
                    else if (!(content is CustomPage) && !(content is MultiRPCPage))
                    {
                        ((MainPage)ContentFrame.Content).btnUpdate.IsEnabled = false;
                    }
                }
            }
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            WindowsContent.BorderThickness = (int)WindowsContent.BorderThickness.Top == 0 ? new Thickness(1) : new Thickness(0);
            if (TaskbarIcon != null)
                TaskbarIcon.TrayToolTip = new ToolTip(!IsActive ? App.Text.ShowMultiRPC : App.Text.HideMultiRPC);
            if (!IsActive)
                TimeWindowWasDeactivated = DateTime.Now;
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (Icon != null && App.Config.HideTaskbarIconWhenMin)
                ShowInTaskbar = WindowState != WindowState.Minimized;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (TaskbarIcon != null)
            {
                TaskbarIcon.Icon = null;
                TaskbarIcon.Dispose();
            }
        }
    }
}
