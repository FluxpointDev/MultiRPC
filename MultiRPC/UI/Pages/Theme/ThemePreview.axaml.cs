using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg;
using MultiRPC.Exceptions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using ShimSkiaSharp;

namespace MultiRPC.UI.Pages.Theme;

public partial class ThemePreview : UserControl
{
    private Theming.Theme _theme;
    public Theming.Theme Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            _theme.Apply(Resources);
        }
    }

    public ThemePreview() => throw new DesignException();
    public ThemePreview(Theming.Theme theme)
    {
        _theme = theme;
        InitializeComponent();
        Theme.Apply(Resources);

        AddSidePage(PageManager.CurrentPages[0], true);
        AddSidePage(PageManager.CurrentPages[1], false);

        txtExample.DataContext = WewTextBox;
        cboExample.Items = WewComboBox;
        cboExample.SelectedIndex = 0;
        tblExample.DataContext = WewTextBlock;
        cbExample.DataContext = WewCheckBox;
        btnExample.DataContext = WewButton;
        btnExampleDisabled.DataContext = WewButtonDisabled;
    }

    private static readonly Language[] WewComboBox =
    {
        Language.GetLanguage(LanguageText.WewComboboxItem),
        Language.GetLanguage(LanguageText.WewComboboxItem2)
    };
        
    private static readonly Language WewTextBox = Language.GetLanguage(LanguageText.WewTextbox);
    private static readonly Language WewTextBlock = Language.GetLanguage(LanguageText.WewTextBlock);
    private static readonly Language WewCheckBox = Language.GetLanguage(LanguageText.WewCheckBox);
    private static readonly Language WewButton = Language.GetLanguage(LanguageText.WewButton);
    private static readonly Language WewButtonDisabled = Language.GetLanguage(LanguageText.WewDisabledButton);
        
    private void UpdateIconColour(SvgSource source, Color color)
    {
        foreach (var commands in source.Picture?.Commands ?? ArraySegment<CanvasCommand>.Empty)
        {
            if (commands is DrawPathCanvasCommand pathCanvasCommand)
            {
                pathCanvasCommand.Paint.Shader = SKShader.CreateColor(new SKColor(color.R, color.G, color.B, color.A), SKColorSpace.Srgb);
            }
        }
    }

    private readonly DisableSettings _disableSetting = SettingManager<DisableSettings>.Setting;
    private void AddSidePage(ISidePage page, bool firstPage)
    {
        var key = page.IconLocation + ".svg";
        var source = AssetManager.LoadSvgImage(key, Theme);
        UpdateIconColour(source, (Color)Resources["ThemeAccentColor3"]!);
        var img = new Image
        {
            Margin = new Thickness(4.5),
            Source = new SvgImage
            {
                Source = source
            }
        };
        var btn = new Button
        {
            Content = img,
            Tag = source
        };
            
        var lang = Language.GetLanguage(page.LocalizableName);
        lang.TextObservable.Subscribe(s => CustomToolTip.SetTip(btn,  _disableSetting.ShowPageTooltips ? null : s));
        _disableSetting.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DisableSettings.ShowPageTooltips))
            {
                CustomToolTip.SetTip(btn, _disableSetting.ShowPageTooltips ? null : lang.Text);
            }
        };

        btn.Classes.Add("nav");
        if (firstPage)
        {
            btn.Classes.Add("selected");
        }
        splPages.Children.Add(btn);
    }
}