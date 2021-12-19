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
            TintOpacity = 0.7
        };
        TintColorProperty.Changed.Subscribe(x =>
        {
            if (x.NewValue.HasValue)
            {
                Material.FallbackColor = TintColor;
            }
        });
    }

    public static readonly StyledProperty<Color> TintColorProperty = AvaloniaProperty.Register<AcrylicBorder, Color>("TintColor");
    public Color TintColor
    {
        get => GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }
}