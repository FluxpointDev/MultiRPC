using System.Reflection;
using Avalonia.Media;

namespace MultiRPC.Theming;

public class Themes
{
    public static readonly Theme Dark = new Theme
    {
        Colours = new Colours
        {
            ThemeAccentColor = Color.FromRgb(54, 57, 62),
            ThemeAccentColor2 = Color.FromRgb(44, 46, 48),
            ThemeAccentColor2Hover = Color.FromRgb(44, 42, 42),
            ThemeAccentColor3 = Color.FromRgb(255, 255, 255),
            ThemeAccentColor4 = Color.FromRgb(180, 180, 180),
            ThemeAccentColor5 = Color.FromRgb(112, 112, 122),
            TextColour = Color.FromRgb(255, 255, 255),

            ThemeAccentDisabledColor = Color.FromRgb(80, 80, 80),
            ThemeAccentDisabledTextColor = Color.FromRgb(255, 255, 255),
            NavButtonSelectedColor = Color.FromRgb(0, 171, 235),
            NavButtonSelectedIconColor = Color.FromRgb(255, 255, 255)
        },
        Metadata = new Metadata("Dark", Assembly.GetExecutingAssembly().GetName().Version!),
    };

    public static readonly Theme Light = new Theme
    {
        Colours = new Colours
        {
            ThemeAccentColor = Color.FromRgb(255, 255, 255),
            ThemeAccentColor2 = Color.FromRgb(239, 242, 243),
            ThemeAccentColor2Hover = Color.FromRgb(234, 234, 234),
            ThemeAccentColor3 = Color.FromRgb(209, 209, 209),
            ThemeAccentColor4 = Color.FromRgb(180, 180, 180),
            ThemeAccentColor5 = Color.FromRgb(112, 112, 112),
            TextColour = Color.FromRgb(112, 112, 112),

            ThemeAccentDisabledColor = Color.FromRgb(80, 80, 80),
            ThemeAccentDisabledTextColor = Color.FromRgb(255, 255, 255),
            NavButtonSelectedColor = Color.FromRgb(0, 171, 235),
            NavButtonSelectedIconColor = Color.FromRgb(255, 255, 255)
        },
        Metadata = new Metadata("Light", Assembly.GetExecutingAssembly().GetName().Version!),
    };
}