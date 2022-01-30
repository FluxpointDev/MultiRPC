using System;
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

namespace MultiRPC.UI.Pages.Theme;

//TODO: Add control for us to let the user tell us if the theme is a light/dark theme
public partial class ThemeEditorPage : UserControl, ITabPage
{
    private ThemePreview _themePreview;
    private Theming.Theme _theme;
    private ColourButton _colourButton;
    private readonly GeneralSettings _generalSettings = SettingManager<GeneralSettings>.Setting;
    
    public Language? TabName { get; } = Language.GetLanguage(LanguageText.ThemeEditor);
    public bool IsDefaultPage => true;
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        _theme = Themes.Dark.Clone(GetInitialName());
        _theme.IsBeingEdited = true;
        txtName.Text = _theme.Metadata.Name;
        _themePreview = new ThemePreview(_theme);
        grdMetadata.Children.Insert(0, _themePreview);

        tblTitle.DataContext = Language.GetLanguage(LanguageText.LetMakeTheme);
        edrPrimary.DataContext = Language.GetLanguage(LanguageText.Primary);
        edrSecondary.DataContext = Language.GetLanguage(LanguageText.Secondary);
        edrButtons.DataContext = Language.GetLanguage(LanguageText.Buttons);
        edrPages.DataContext = Language.GetLanguage(LanguageText.Pages);
        edrOther.DataContext = Language.GetLanguage(LanguageText.Other);

        btnSave.DataContext = Language.GetLanguage(LanguageText.SaveTheme);
        btnSaveAndApply.DataContext = Language.GetLanguage(LanguageText.SaveAndApplyTheme);
        btnReset.DataContext = Language.GetLanguage(LanguageText.ResetTheme);
        txtName.DataContext = Language.GetLanguage(LanguageText.ThemeName);
        
        clbColour1.Language = Language.GetLanguage(LanguageText.Colour1);
        clbColour2.Language = Language.GetLanguage(LanguageText.Colour2);
        clbColour2Hover.Language = Language.GetLanguage(LanguageText.Colour2Hover);
        clbColour3.Language = Language.GetLanguage(LanguageText.Colour3);
        clbColour4.Language = Language.GetLanguage(LanguageText.Colour4);
        clbColour5.Language = Language.GetLanguage(LanguageText.Colour5);
        clbTextColour.Language = Language.GetLanguage(LanguageText.TextColour);
        clbDisabledBtnColour.Language = Language.GetLanguage(LanguageText.DisabledButtonColour);
        clbDisabledBtnTextColour.Language = Language.GetLanguage(LanguageText.DisabledButtonTextColour);
        clbSelectedPageColour.Language = Language.GetLanguage(LanguageText.SelectedPageColour);
        clbSelectedPageIconColour.Language = Language.GetLanguage(LanguageText.SelectedPageIconColour);

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
        clpPicker.FindControl<ComboBox>("ColorSpaceComboBox").SelectedIndex = 1;
        clpPicker.FindControl<IControl>("PaletteSelector").IsVisible = false;
        
        var grid = (Grid)((Grid)clpPicker.Content).Children[0];
        grid.Children[2].IsVisible = false;
        grid.Children[6].IsVisible = false;

        var zeroL = GridLength.Parse("0");
        grid.ColumnDefinitions[3].Width = GridLength.Auto;
        grid.ColumnDefinitions[4].Width = zeroL;
        grid.ColumnDefinitions[^1].Width = GridLength.Auto;
        grid.ColumnDefinitions[^2].Width = zeroL;

        var hsvControls = ((Grid)grid.Children[4]).Children;
        for (int i = 7; i < hsvControls.Count; i++)
        {
            hsvControls[i].IsVisible = false;
        }
        var clabControls = ((Grid)grid.Children[5]).Children;
        for (int i = 0; i < 10; i++)
        {
            clabControls[i].IsVisible = false;
        }

        /*Move the hex controls to the next grid controls,
         we're have more space that way*/
        var hexHeader = (Control)clabControls[10];
        Grid.SetRow(hexHeader, 4);
        clabControls.RemoveAt(10);

        var hexHashText = (Control)clabControls[10];
        Grid.SetRow(hexHashText, 5);
        clabControls.RemoveAt(10);

        var hexTextBox = (TextBox)clabControls[10];
        Grid.SetRow(hexTextBox, 5);
        clabControls.RemoveAt(10);

        hsvControls.Add(hexHeader);
        hsvControls.Add(hexHashText);
        hsvControls.Add(hexTextBox);
        
        hexTextBox.GetObservable(TextBox.TextProperty).Subscribe(x =>
        {
            _colourButton.BtnColor = new ImmutableSolidColorBrush(clpPicker.Color);
            UpdateTheme(_colourButton.Name, clpPicker.Color);
        });
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
        _theme = Themes.Dark.Clone(GetInitialName());
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