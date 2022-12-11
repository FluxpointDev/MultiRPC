using System.Text.Json.Serialization;
using Avalonia.Media;

namespace MultiRPC.Theming;

public class Colours
{
    [JsonPropertyName("AccentColour1")]
    public Color ThemeAccentColor { get; set; }

    [JsonPropertyName("AccentColour2")]
    public Color ThemeAccentColor2 { get; set; }
    
    [JsonPropertyName("AccentColour2Hover")]
    public Color ThemeAccentColor2Hover { get; set; }
    
    [JsonPropertyName("AccentColour3")]
    public Color ThemeAccentColor3 { get; set; }
    
    [JsonPropertyName("AccentColour4")]
    public Color ThemeAccentColor4 { get; set; }
    
    [JsonPropertyName("AccentColour5")]
    public Color ThemeAccentColor5 { get; set; }

    [JsonPropertyName("DisabledButtonColour")]
    public Color ThemeAccentDisabledColor { get; set; }
    
    [JsonPropertyName("DisabledButtonTextColour")]
    public Color ThemeAccentDisabledTextColor { get; set; }

    [JsonPropertyName("NavButtonBackgroundSelected")]
    public Color NavButtonSelectedColor { get; set; }
    
    [JsonPropertyName("NavButtonIconColourSelected")]
    public Color NavButtonSelectedIconColor { get; set; }
    
    [JsonPropertyName("TextColour")]
    public Color TextColour { get; set; }
}