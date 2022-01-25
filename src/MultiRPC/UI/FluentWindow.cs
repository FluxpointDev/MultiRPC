using Avalonia.Styling;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Controls.Primitives;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI;

//Taken from https://github.com/AvaloniaUI/XamlControlsGallery/blob/master/XamlControlsGallery/FluentWindow.cs
public class FluentWindow : Window, IStyleable
{
    Type IStyleable.StyleKey => typeof(Window);

    private bool _disableMinimiseButton = false;
    public bool DisableMinimiseButton
    {
        get => _disableMinimiseButton;
        set
        {
            _disableMinimiseButton = value;
            UpdateMinimiseButton(!value);
        }
    }
    
    private bool _disableRestoreButton = false;
    public bool DisableRestoreButton
    {
        get => _disableRestoreButton;
        set
        {
            _disableRestoreButton = value;
            UpdateRestoreButton(!value);
        }
    }

    private static readonly DisableSettings _disableSettings = SettingManager<DisableSettings>.Setting;
    public FluentWindow()
    {
        Title = Language.GetText(LanguageText.MultiRPC);
        _disableSettings.PropertyChanged += UpdateTransparencyLevel;
        UpdateTransparencyLevel(null, new PropertyChangedEventArgs(nameof(DisableSettings.AcrylicEffect)));
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = -1;


        this.GetObservable(WindowStateProperty)
            .Subscribe(x =>
            {
                PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
            });

        this.GetObservable(IsExtendedIntoWindowDecorationsProperty)
            .Subscribe(x =>
            {
                if (!IsInitialized)
                {
                    return;
                }

                UpdateRestoreButton(!DisableRestoreButton);
                UpdateMinimiseButton(!DisableMinimiseButton);
                SystemDecorations = SystemDecorations.Full;
                if (!x)
                {
                    TransparencyLevelHint = WindowTransparencyLevel.None;
                    _disableSettings.PropertyChanged -= UpdateTransparencyLevel;
                }
            });
    }

    private void UpdateTransparencyLevel(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DisableSettings.AcrylicEffect))
        {
            return;
        }
        if (_disableSettings.AcrylicEffect)
        {
            TransparencyLevelHint = WindowTransparencyLevel.None;
            return;
        }
            
        if (OperatingSystem.IsWindows())
        {
            var version = Environment.OSVersion.Version;
            if (version.Major >= 10)
            {
                TransparencyLevelHint = version.Build >= 22000 ? WindowTransparencyLevel.Mica : WindowTransparencyLevel.AcrylicBlur;
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
        }
    }

    protected virtual void UpdateRestoreButton(bool shouldEnable) { }

    protected virtual void UpdateMinimiseButton(bool shouldEnable) { }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);            
        ExtendClientAreaChromeHints =
            ExtendClientAreaChromeHints.PreferSystemChrome |
            ExtendClientAreaChromeHints.OSXThickTitleBar;
    }
}