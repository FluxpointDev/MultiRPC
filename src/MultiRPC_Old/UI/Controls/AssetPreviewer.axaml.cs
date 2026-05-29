using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MultiRPC.Theming;

namespace MultiRPC.UI.Controls;

public class AssetPreviewer : StackPanel
{
    private AssetControl _assetControl;

    public AssetPreviewer() : this("Icons/Discord.svg"){ }
    public AssetPreviewer(string key, Theme? theme = null)
    {
        InitializeComponent(key, theme);
    }

    public void SetTheme(Theme theme)
    {
        _assetControl.SetTheme(theme);
    }
    
    private void InitializeComponent(string key, Theme? theme)
    {
        Margin = new Thickness(0, 0, 10, 10);
        _assetControl = new AssetControl(key, theme)
        {
            Margin = new Thickness(10),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        Children.Add(new Border
        {
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.Black,
            CornerRadius = new CornerRadius(10),
            Height = 150,
            Width = 150,
            Child = _assetControl,
        });
        Children.Add(new TextBlock
        {
            Text = Path.GetFileNameWithoutExtension(key),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0,8,0,0)
        });
    }
}