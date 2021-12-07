using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme
{
    public class MasterThemeEditorPage : SidePage
    {
        public override string IconLocation => "Icons/ThemeEditor";
        public override string LocalizableName => "ThemeEditor";
        public override void Initialize(bool loadXaml)
        {
            Background = (IBrush)Application.Current.Resources["ThemeAccentBrush"]!;
            var tabPage = new TabsPage
            {
                Width = 675,
                Height = 520
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
}