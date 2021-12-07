using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Svg;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
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
            Closing += (sender, args) => trayIcon.IsVisible = false;

            Language.LanguageChanged += (sender, args) => ChangeTrayIconText(trayIcon);
            this.GetObservable(WindowStateProperty).Subscribe(x =>
            {
                ChangeTrayIconText(trayIcon);
                ShowInTaskbar = !(!disableSettings.HideTaskbarIcon && x == WindowState.Minimized);
            });
            
            DragDrop.SetAllowDrop(this, true);
            AddHandler(DragDrop.DropEvent, DragOver);
            AddHandler(DragDrop.DragEnterEvent, DragEnter);
        }

        private void DragEnter(object? sender, DragEventArgs e)
        {
            // Only allow Copy or Link as Drop Operations.
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

            // Only allow if the dragged data contains text or filenames.
            if (!e.Data.Contains(DataFormats.FileNames))
                e.DragEffects = DragDropEffects.None;
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            // Only allow if the dragged data contains filenames.
            if (!e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            var file = e.Data.GetFileNames()?.Last();
            var ext = Path.GetExtension(file);
            if (ext is Constants.ThemeFileExtension or Constants.LegacyThemeFileExtension)
            {
                Theme.Load(file)?.Apply();
            }
        }
        
        private readonly Control _control;
        public MainWindow(Control control)
        {
            Icon = new WindowIcon(AssetManager.GetSeekableStream("Logo.ico"));
            AssetManager.RegisterForAssetReload("Logo.ico", 
                () => Icon = new WindowIcon(AssetManager.GetSeekableStream("Logo.ico")));
            
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
            AssetManager.RegisterForAssetReload("Logo.svg", () =>
            {
                icon.Source = new SvgImage { Source = AssetManager.LoadSvgImage("Logo.svg") };
            });
            icon.Source = new SvgImage
            {
                Source = AssetManager.LoadSvgImage("Logo.svg")
            };
            
            var lang = Language.GetLanguage(LanguageText.MultiRPC);
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
