using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using MultiRPC.Extensions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme.Editor;

//TODO: Add control for us to let the user tell us if the theme is a light/dark theme
public partial class ThemeEditorPage : StackPanel, ITabPage
{
    private ThemePreview _themePreview;
    private Theming.Theme _theme;
    private ColourButton _colourButton;
    private readonly GeneralSettings _generalSettings = SettingManager<GeneralSettings>.Setting;
    
    public Language? TabName { get; } = LanguageText.Editor;
    public bool IsDefaultPage => true;
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        _theme = Themes.Default.Clone(GetInitialName());
        _theme.IsBeingEdited = true;
        txtName.Text = _theme.Metadata.Name;
        _themePreview = new ThemePreview(_theme);
        grdMetadata.Children.Insert(0, _themePreview);

        tblTitle.DataContext = (Language)LanguageText.LetMakeTheme;
        edrPrimary.DataContext = (Language)LanguageText.Primary;
        edrSecondary.DataContext = (Language)LanguageText.Secondary;
        edrButtons.DataContext = (Language)LanguageText.Buttons;
        edrPages.DataContext = (Language)LanguageText.Pages;
        edrOther.DataContext = (Language)LanguageText.Other;

        btnSave.DataContext = (Language)LanguageText.SaveTheme;
        btnSaveAndApply.DataContext = (Language)LanguageText.SaveAndApplyTheme;
        btnReset.DataContext = (Language)LanguageText.ResetTheme;
        txtName.DataContext = (Language)LanguageText.ThemeName;
        
        clbColour1.Language = LanguageText.Colour1;
        clbColour2.Language = LanguageText.Colour2;
        clbColour2Hover.Language = LanguageText.Colour2Hover;
        clbColour3.Language = LanguageText.Colour3;
        clbColour4.Language = LanguageText.Colour4;
        clbColour5.Language = LanguageText.Colour5;
        clbTextColour.Language = LanguageText.TextColour;
        clbDisabledBtnColour.Language = LanguageText.DisabledButtonColour;
        clbDisabledBtnTextColour.Language = LanguageText.DisabledButtonTextColour;
        clbSelectedPageColour.Language = LanguageText.SelectedPageColour;
        clbSelectedPageIconColour.Language = LanguageText.SelectedPageIconColour;

        _colourButton = clbColour1;
        SetColours();
        clpPicker.Color = _colourButton.BtnColor.Color;
        ProcessColourPicker();
    }

    public void EditTheme(Theming.Theme theme)
    {
        _theme.IsBeingEdited = false;
        _theme = theme;
        _theme.IsBeingEdited = true;
        SetupForNewTheme();
    }

    private string GetInitialName() => Language.GetText(LanguageText.ThemeName);

    /*Gets rid of all the controls in the colour picker that we don't want,
     while they are cool... there'll also be very confusing to the average user*/
    private void ProcessColourPicker()
    {
        var grid = (Grid)((Grid)clpPicker.Content).Children[0];
        var clabControls = ((Grid)grid.Children[5]).Children;
        var hexTextBox = clpPicker.Find<TextBox>("Hex_Box");
        
        Grid.SetRow(hexTextBox, 5);
        clabControls.Remove(hexTextBox);

        var hexHeader = (Control)clabControls[10];
        Grid.SetRow(hexHeader, 4);
        clabControls.Remove(hexHeader);
        
        var hexHashText = (Control)clabControls[10];
        Grid.SetRow(hexHashText, 5);
        clabControls.Remove(hexHashText);

        var hsvControls = ((Grid)grid.Children[4]).Children;
        hsvControls.Add(hexHashText);
        hsvControls.Add(hexHeader);
        hsvControls.Add(hexTextBox);

        hexTextBox.GetObservable(TextBox.TextProperty).Subscribe(x =>
        {
            _colourButton.BtnColor = new ImmutableSolidColorBrush(clpPicker.Color);
            UpdateTheme(_colourButton.Name, clpPicker.Color);
        });
        
        clpPicker.IsPaletteVisible = false;
        clpPicker.IsHSBVisible = false;
        clpPicker.IsAlphaVisible = false;
        clpPicker.IsColourBlindnessSelectorVisible = false;
        clpPicker.IsCIELABVisible = false;
        
        var zeroL = GridLength.Parse("0");
        grid.ColumnDefinitions[^1].Width = GridLength.Auto;
        grid.ColumnDefinitions[^2].Width = zeroL;
    }

    private void UpdateTheme(string name, Color color)
    {
        switch (name)
        {
            case "clbColour1":
                _theme.Colours.ThemeAccentColor = color;
                break;
            case "clbColour2":
                _theme.Colours.ThemeAccentColor2 = color;
                break;
            case "clbColour2Hover":
                _theme.Colours.ThemeAccentColor2Hover = color;
                break;
            case "clbColour3":
                _theme.Colours.ThemeAccentColor3 = color;
                break;
            case "clbColour4":
                _theme.Colours.ThemeAccentColor4 = color;
                break;
            case "clbColour5":
                _theme.Colours.ThemeAccentColor5 = color;
                break;
            case "clbTextColour":
                _theme.Colours.TextColour = color;
                break;
            case "clbDisabledBtnColour":
                _theme.Colours.ThemeAccentDisabledColor = color;
                break;
            case "clbDisabledBtnTextColour":
                _theme.Colours.ThemeAccentDisabledTextColor = color;
                break;
            case "clbSelectedPageColour":
                _theme.Colours.NavButtonSelectedColor = color;
                break;
            case "clbSelectedPageIconColour":
                _theme.Colours.NavButtonSelectedIconColor = color;
                break;
        }
        _theme.Apply(_themePreview.Resources, false);
    }

    private void SetColours()
    {
        clbColour1.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentColor);
        clbColour2.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentColor2);
        clbColour2Hover.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentColor2Hover);
        clbColour3.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentColor3);
        clbColour4.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentColor4);
        clbColour5.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentColor5);
        clbTextColour.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.TextColour);
        clbDisabledBtnColour.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentDisabledColor);
        clbDisabledBtnTextColour.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.ThemeAccentDisabledTextColor);
        clbSelectedPageColour.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.NavButtonSelectedColor);
        clbSelectedPageIconColour.BtnColor = new ImmutableSolidColorBrush(_theme.Colours.NavButtonSelectedIconColor);
    }

    private void ClbColour_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _colourButton = (ColourButton)sender!;
        clpPicker.Color = _colourButton.BtnColor.Color;
    }

    private void BtnSave_OnClick(object? sender, RoutedEventArgs e)
    {
        var filename = string.IsNullOrWhiteSpace(_theme.Location) 
            ? FileExt.CheckFilename(txtName.Text, Constants.ThemeFolder) : null;
        _theme.Metadata.Name = txtName.Text;
        _theme.Save(filename);
        _theme.IsBeingEdited = false;
        BtnReset_OnClick(sender, e);
    }

    private void BtnSaveAndApply_OnClick(object? sender, RoutedEventArgs e)
    {
        var filename = FileExt.CheckFilename(txtName.Text, Constants.ThemeFolder);
        _theme.Metadata.Name = txtName.Text;
        _theme.Save(filename);
        _theme.IsBeingEdited = false;
        _theme.Apply();
        _generalSettings.ThemeFile = _theme.Location;
        BtnReset_OnClick(sender, e);
    }

    private void BtnReset_OnClick(object? sender, RoutedEventArgs e)
    {
        _theme.IsBeingEdited = false;
        _theme = Themes.Default.Clone(GetInitialName());
        _theme.IsBeingEdited = true;
        SetupForNewTheme();
    }

    private void SetupForNewTheme()
    {
        _themePreview.Theme = _theme;
        SetColours();
        clpPicker.Color = _colourButton.BtnColor.Color;
        txtName.Text = _theme.Metadata.Name;
    }
}