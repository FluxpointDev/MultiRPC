using Avalonia;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Pages.Theme.Editor;

namespace MultiRPC.UI.Pages.Theme;

public class MasterThemeEditorPage : TabsPage, ISidePage
{
    public string IconLocation => "Icons/ThemeEditor";
    public string LocalizableName => "ThemeEditor";
    public Thickness ContentPadding { get; } = new Thickness(0);

    public override void Initialize(bool loadXaml)
    {
        MinWidth = 675;
        MaxWidth = 997.5;
        MaxHeight = 680;

        var editorPage = new ThemeEditorPage
        { Margin = new Thickness(10, 0) };

        AddTab(MakeEditorTabs(editorPage));
        AddTab(new InstalledThemesPage(editorPage));
        base.Initialize(loadXaml);
    }

    private TabsPage MakeEditorTabs(ThemeEditorPage editorPage)
    {
        var tabPage = new TabsPage { TabName = LanguageText.ThemeEditor };
        tabPage.AddTab(editorPage);
        tabPage.AddTab(new ThemeAssetPage { Margin = new Thickness(10, 0) });

        return tabPage;
    }
}