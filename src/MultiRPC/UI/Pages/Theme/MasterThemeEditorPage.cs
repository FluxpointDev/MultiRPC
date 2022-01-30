using Avalonia;
using Avalonia.Media;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme;

public class MasterThemeEditorPage : SidePage
{
    public override string IconLocation => "Icons/ThemeEditor";
    public override string LocalizableName => "ThemeEditor";
    public override void Initialize(bool loadXaml)
    {
        var tabPage = new TabsPage
        {
            MinWidth = 675,
            MaxWidth = 997.5,
            MaxHeight = 680
        };
        ContentPadding = new Thickness(0);

        var editorPage = new ThemeEditorPage
        {
            Margin = new Thickness(10, 0)
        };
        tabPage.AddTab(editorPage);
        tabPage.AddTab(new InstalledThemesPage(editorPage));
        tabPage.Initialize();
        Content = tabPage;
    }
}