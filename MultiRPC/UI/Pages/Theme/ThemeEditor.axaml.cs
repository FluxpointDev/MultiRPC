using Avalonia.Controls;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme;

public partial class ThemeEditorPage : UserControl, ITabPage
{
    public Language? TabName { get; } = Language.GetLanguage("ThemeEditor");
    public bool IsDefaultPage => true;
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);
    }
}