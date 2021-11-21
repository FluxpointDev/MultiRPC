using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Settings
{
    public partial class AboutSettingsTab : UserControl, ITabPage
    {
        public AboutSettingsTab()
        {
            InitializeComponent();
        }

        public Language? TabName { get; } = new Language("About");
        public bool IsDefaultPage => true;
        public void Initialize(bool loadXaml)
        {
            throw new System.NotImplementedException();
        }
    }
}