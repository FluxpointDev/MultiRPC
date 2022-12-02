using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Svg;
using Avalonia.VisualTree;
using MultiRPC.Commands;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Utils;

namespace MultiRPC.UI;

public partial class MainWindow : FluentWindow
{
    private readonly Control _control;
    private readonly DisableSettings _disableSettings = SettingManager<DisableSettings>.Setting;
    public MainWindow() : this(new MainPage())
    {
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, DragOver);
        AddHandler(DragDrop.DragEnterEvent, DragEnter);

        /*Tray Icon seems to be a bit broken on anything other then windows
         after running for a while, skip if we aren't on windows but we also 
         crash on shutdown which is an issue for passing Microsoft store validation 
         so skip if deploying to it*/
#if !_UWP
        if (!OperatingSystem.IsWindows())
#endif
        {
            return;
        }

        var trayIcon = new TrayIcon
        {
            Icon = this.Icon,
            ToolTipText = Language.GetText(LanguageText.HideMultiRPC),
            Command = new TrayCommand()
        };
        Closing += (sender, args) => trayIcon.IsVisible = false;

        LanguageGrab.LanguageChanged += (sender, args) => ChangeTrayIconText(trayIcon);
        this.GetObservable(WindowStateProperty).Subscribe(x =>
        {
            ChangeTrayIconText(trayIcon);
            ShowInTaskbar = !(!_disableSettings.HideTaskbarIcon && x == WindowState.Minimized);
        });
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        // Only allow Copy or Link as Drop Operations.
        e.DragEffects &= DragDropEffects.Copy | DragDropEffects.Link;

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
            Theming.Theme.Load(file)?.Apply();
        }
    }

    //TODO: Check that this works on other OS's
    protected override void UpdateMinimiseButton(bool shouldEnable)
    {
        if (IsExtendedIntoWindowDecorations)
        {
            UpdateTitleBar(shouldEnable, "PART_MinimiseButton");
        }
        //TODO: Add for other OS's
    }

    protected override void UpdateRestoreButton(bool shouldEnable)
    {
        if (IsExtendedIntoWindowDecorations)
        {
            UpdateTitleBar(shouldEnable, "PART_RestoreButton");
        }
        //TODO: Add for other OS's
    }

    private void UpdateTitleBar(bool shouldEnable, string controlName)
    {
        UpdateTitleBar(tbrTitleBar, shouldEnable, controlName);
        if (this.GetVisualChildren().FirstOrDefault()?.GetVisualChildren().LastOrDefault() is VisualLayerManager visualLayerManager
            && visualLayerManager.ChromeOverlayLayer.Children.FirstOrDefault() is TitleBar titleBar)
        {
            UpdateTitleBar(titleBar, shouldEnable, controlName);
        }
    }
    
    private void UpdateTitleBar(IVisual? title, bool shouldEnable, string controlName)
    {
        var cap = title?.GetVisualChildren()
            .FirstOrDefault()
            ?.GetVisualChildren()
            .LastOrDefault()
            ?.GetVisualChildren()
            .LastOrDefault() as CaptionButtons;

        if (cap != null)
        {
            var stp = cap.GetVisualChildren().Single() as StackPanel;
            stp.Children.First(x => x.Name == controlName).IsVisible = shouldEnable;
        }
    }

    public MainWindow(Control control)
    {
        DisableRestoreButton = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Icon = new WindowIcon(AssetManager.GetSeekableStream("Logo.ico"));
        AssetManager.RegisterForAssetReload("Logo.ico", 
            () => Icon = new WindowIcon(AssetManager.GetSeekableStream("Logo.ico")));
            
        _control = control;
        InitializeComponent();
        InitializeExtra();
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
        Title = txtTitle.Text;
    }

    private void InitializeExtra()
    {
        _disableSettings.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DisableSettings.AcrylicEffect))
            {
                regBackground.Opacity = _disableSettings.AcrylicEffect ? 1 : 0.85;
            }
        };
        regBackground.Opacity = _disableSettings.AcrylicEffect ? 1 : 0.85;

        if (!OperatingSystem.IsMacOS())
        {
            AssetManager.RegisterForAssetReload("Logo.svg", () =>
            {
                icon.Source = new SvgImage { Source = AssetManager.LoadSvgImage("Logo.svg") };
            });
            icon.Source = new SvgImage
            {
                Source = AssetManager.LoadSvgImage("Logo.svg")
            };
        }

        stpTitleBarContent.PointerPressed += (sender, args) => BeginMoveDrag(args);
        Opened += (sender, args) =>
        {
            stpTitleBarContent.Height = WindowDecorationMargin.Top;
            tbrTitleBar.Height = stpTitleBarContent.Height;
            icon.Height = stpTitleBarContent.Height - icon.Margin.Top - icon.Margin.Bottom;
            icon.Width = icon.Height;
            _control.Margin += new Thickness(0, stpTitleBarContent.Height, 0, 0);
        };
        grdContent.Children.Insert(3, _control);

        Language lang = LanguageText.MultiRPC;
        if (_control is ITitlePage titlePage)
        {
            lang.TextObservable.Subscribe(s => UpdateTitle(s, titlePage.Title.Text));
            titlePage.Title.TextObservable.Subscribe(s => UpdateTitle(lang.Text, s));
        }
        else
        {
            lang.TextObservable.Subscribe(s => UpdateTitle(s, null));
        }
        
        if (OperatingSystem.IsMacOS())
        {
            icon.IsVisible = false;
            txtTitle.Margin = new Thickness(0);
            stpTitleBarContent.HorizontalAlignment = HorizontalAlignment.Center;
        }
    }
}