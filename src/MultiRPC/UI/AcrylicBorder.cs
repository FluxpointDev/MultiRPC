using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MultiRPC.UI;

public class AcrylicBorder : ExperimentalAcrylicBorder
{
    public AcrylicBorder()
    {
        Material = new ExperimentalAcrylicMaterial
        {
            MaterialOpacity = 0.8,
            TintOpacity = 1,
            TintColor = TintColor,
            FallbackColor = TintColor
        };
        TintColorProperty.Changed.Subscribe(x =>
        {
            if (x.NewValue.HasValue)
            {
                Material.FallbackColor = TintColor;
                Material.TintColor = TintColor;
            }
        });
    }

    public static readonly StyledProperty<Color> TintColorProperty = AvaloniaProperty.Register<AcrylicBorder, Color>("TintColor", (Color)App.Current.Resources["ThemeAccentColor"]);
    public Color TintColor
    {
        get => GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }
}