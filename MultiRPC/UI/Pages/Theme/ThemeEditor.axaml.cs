using System;
using System.Reflection;
using Avalonia.Controls;
using AvaloniaColorPicker;
using MultiRPC.Theming;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme;

public partial class ThemeEditorPage : UserControl, ITabPage
{
    private ThemePreview _themePreview;
    private Theming.Theme _theme;
    
    public Language? TabName { get; } = Language.GetLanguage("ThemeEditor");
    public bool IsDefaultPage => true;
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        _theme = Themes.Dark.Clone("wew3");
        _themePreview = new ThemePreview(_theme);
        gidPreview.Children.Insert(0, _themePreview);

        /*Get rid of all the controls in the colour picker that we don't want,
         while they are cool... there'll also be very confusing to the average user*/
        clpPicker.FindControl<IControl>("PaletteSelector").IsVisible = false;
        clpPicker.FindControl<ComboBox>("ColorSpaceComboBox").SelectedIndex = 1;
        
        var grid = (Grid)((Grid)clpPicker.Content).Children[0];
        grid.Children[2].IsVisible = false;
        grid.ColumnDefinitions[3].Width = GridLength.Auto;

        var hsvControls = ((Grid)grid.Children[4]).Children;
        for (int i = 7; i < hsvControls.Count; i++)
        {
            hsvControls[i].IsVisible = false;
        }
        var clabControls = ((Grid)grid.Children[5]).Children;
        for (int i = 0; i < 7; i++)
        {
            clabControls[i].IsVisible = false;
        }
        grid.Children[6].IsVisible = false;
    }
}