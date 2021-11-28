using System.Text.Json.Serialization;
using Avalonia.Media;

namespace MultiRPC.Theming;
public class Colours
{
    [JsonPropertyName("AccentColour1")]
    public Color ThemeAccentColor { get; init; }

    [JsonPropertyName("AccentColour2")]
    public Color ThemeAccentColor2 { get; init; }
    
    [JsonPropertyName("AccentColour2Hover")]
    public Color ThemeAccentColor2Hover { get; init; }
    
    [JsonPropertyName("AccentColour3")]
    public Color ThemeAccentColor3 { get; init; }
    
    [JsonPropertyName("AccentColour4")]
    public Color ThemeAccentColor4 { get; init; }
    
    [JsonPropertyName("AccentColour5")]
    public Color ThemeAccentColor5 { get; init; }

    [JsonPropertyName("DisabledButtonColour")]
    public Color ThemeAccentDisabledColor { get; init; }
    
    [JsonPropertyName("DisabledButtonTextColour")]
    public Color ThemeAccentDisabledTextColor { get; init; }

    [JsonPropertyName("NavButtonBackgroundSelected")]
    public Color NavButtonSelectedColor { get; init; }
    
    [JsonPropertyName("NavButtonIconColourSelected")]
    public Color NavButtonSelectedIconColor { get; init; }
    
    [JsonPropertyName("TextColour")]
    public Color TextColour { get; init; }
}