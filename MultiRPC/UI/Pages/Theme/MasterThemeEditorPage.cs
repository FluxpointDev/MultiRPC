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
        Background = (IBrush)Application.Current.Resources["ThemeAccentBrush"]!;
        var tabPage = new TabsPage
        {
            MinWidth = 675,
            MinHeight = 520
        };
        ContentPadding = new Thickness(0);
        tabPage.AddTabs(new ThemeEditorPage()
        {
            Margin = new Thickness(10)
        }, new InstalledThemes());
        tabPage.Initialize();
        Content = tabPage;
    }
}