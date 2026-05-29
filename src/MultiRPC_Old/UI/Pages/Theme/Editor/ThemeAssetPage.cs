using Avalonia;
using Avalonia.Controls;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme.Editor;

public class ThemeAssetPage : WrapPanel, ITabPage
{
    public ThemeAssetPage()
    {
        Initialize(true);
    }

    public Language? TabName { get; } = LanguageText.Assets;
    public bool IsDefaultPage { get; }
    public void Initialize(bool _)
    {
        Margin = new Thickness(10);
        foreach (var asset in AssetManager.GetAllAssets())
        {
            //TODO: Add Theming
            var themeControl = new AssetPreviewer(asset[8..]);
            Children.Add(themeControl);
        }
    }
}