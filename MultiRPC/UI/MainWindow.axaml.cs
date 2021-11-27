using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Utils;

namespace MultiRPC.UI
{
    class TrayCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            var mainWin = ((App)Application.Current).DesktopLifetime!.MainWindow!;
            switch (mainWin.WindowState)
            {
                case WindowState.Normal:
                case WindowState.Maximized:
                case WindowState.FullScreen:
                    mainWin.WindowState = WindowState.Minimized;
                    break;
                case WindowState.Minimized:
                    mainWin.WindowState = WindowState.Normal;
                    return;
                default:
                    return;
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
    
    public partial class MainWindow : FluentWindow
    {
        public MainWindow() : this(new MainPage())
        {
            var disableSettings = SettingManager<DisableSettings>.Setting;
            var trayIcon = new TrayIcon
            {
                Icon = this.Icon,
                ToolTipText = Language.GetText(LanguageText.HideMultiRPC),
                Command = new TrayCommand()
            };
            TrayIcon.SetIcons(this, new TrayIcons { trayIcon });
            Closing += (sender, args) =>
            {
                TrayIcon.GetIcons(this)[0].IsVisible = false;
            };

            Language.LanguageChanged += (sender, args) => ChangeTrayIconText(trayIcon);
            this.GetObservable(WindowStateProperty).Subscribe(x =>
            {
                ChangeTrayIconText(trayIcon);
                ShowInTaskbar = !(!disableSettings.HideTaskbarIcon && x == WindowState.Minimized);
            });
        }
        
        private readonly Control _control;
        public MainWindow(Control control)
        {
            _control = control;
            InitializeComponent();
            InitializeExtra();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        private void ChangeTrayIconText(TrayIcon trayIcon)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                case WindowState.Maximized:
                case WindowState.FullScreen:
                    trayIcon.ToolTipText = Language.GetText(LanguageText.HideMultiRPC);
                    break;
                case WindowState.Minimized:
                    trayIcon.ToolTipText = Language.GetText(LanguageText.ShowMultiRPC);
                    return;
                default:
                    return;
            }
        }

        private void UpdateTitle(string title, string? subTitle)
        {
            txtTitle.Text = title + (subTitle != null ? " - " + subTitle : null) + (AdminUtil.IsAdmin ? $" ({Language.GetText(LanguageText.Administrator)})" : null);
        }

        private void InitializeExtra()
        {
            var lang = new Language(LanguageText.MultiRPC);
            if (_control is ITitlePage titlePage)
            {
                lang.TextObservable.Subscribe(s => UpdateTitle(s, titlePage.Title.Text));
                titlePage.Title.TextObservable.Subscribe(s => UpdateTitle(lang.Text, s));
            }
            else
            {
                lang.TextObservable.Subscribe(s => UpdateTitle(s, null));
            }
            
            eabTitleBar.PointerPressed += (sender, args) => BeginMoveDrag(args);
            Opened += (sender, args) =>
            {
                eabTitleBar.Height = WindowDecorationMargin.Top;
                tbrTitleBar.Height = eabTitleBar.Height;
                icon.Height = eabTitleBar.Height - icon.Margin.Top - icon.Margin.Bottom;
                icon.Width = icon.Height;
                _control.Margin += new Thickness(0, eabTitleBar.Height, 0, 0);
            };
            grdContent.Children.Insert(1, _control);
        }
    }

    public static class MainWindowExt
    {
        public static bool TryClose<T>(this UserControl userControl, T result)
        {
            if (userControl.Parent?.Parent is MainWindow window)
            {
                window.Close(result);
                return true;
            }

            return false;
        }
        
        public static bool TryClose(this UserControl userControl)
        {
            if (userControl.Parent?.Parent is MainWindow window)
            {
                window.Close();
                return true;
            }

            return false;
        }
    }
}
