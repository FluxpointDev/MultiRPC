using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme.Editor;

public partial class ThemeAssetPage : UserControl, ITabPage
{
    public ThemeAssetPage()
    {
        Initialize(true);
    }

    public Language? TabName { get; } = LanguageText.Assets;
    public bool IsDefaultPage { get; }
    public void Initialize(bool loadXaml)
    {
        InitializeComponent();
    }
}