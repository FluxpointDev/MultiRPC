using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Svg;
using MultiRPC.Exceptions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using ShimSkiaSharp;

namespace MultiRPC.UI.Pages.Theme;

public partial class ThemePreview : Grid
{
    private Theming.Theme _theme;
    private static readonly Language WewTextBox = LanguageText.WewTextbox;
    private static readonly Language WewTextBlock = LanguageText.WewTextBlock;
    private static readonly Language WewCheckBox = LanguageText.WewCheckBox;
    private static readonly Language WewButton = LanguageText.WewButton;
    private static readonly Language WewButtonDisabled = LanguageText.WewDisabledButton;
    private readonly DisableSettings _disableSetting = SettingManager<DisableSettings>.Setting;

    public new Theming.Theme Theme
    {
        get => _theme;
        set
        {
            _theme.PropertyChanged -= EditingThemeOnIsEditingChanged;
            _theme = value;
            _theme.PropertyChanged += EditingThemeOnIsEditingChanged;
            _theme.Apply(Resources);
        }
    }
    
    private void EditingThemeOnIsEditingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Theming.Theme.IsBeingEdited)
            && !Theme.IsBeingEdited)
        {
            Theme.Apply(Resources);
        }
    }
    
    public ThemePreview() => throw new DesignException();
    public ThemePreview(Theming.Theme theme)
    {
        _theme = theme;
        _theme.PropertyChanged += EditingThemeOnIsEditingChanged;

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
        LanguageText.WewComboboxItem,
        LanguageText.WewComboboxItem2
    };
    
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

    private void AddSidePage(ISidePage page, bool firstPage)
    {
        var key = page.IconLocation + ".svg";
        var source = AssetManager.LoadSvgImage(key, Theme);

        var colourName = firstPage ? "NavButtonSelectedIconColor" : "ThemeAccentColor3";
        UpdateIconColour(source, (Color)Resources[colourName]!);
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
        
        this.GetResourceObservable(colourName).Subscribe(x =>
        {
            UpdateIconColour(source, (Color)x!);
            ((Image)btn.Content).Source = new SvgImage
            {
                Source = source
            };
        });

        Language lang = page.LocalizableName;
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