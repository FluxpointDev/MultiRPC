using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Theme
{
    public partial class ThemeEditorPage : UserControl, ITabPage
    {
        public Language? TabName { get; } = new Language("ThemeEditor");
        public bool IsDefaultPage => true;
        public void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);
        }
    }
}