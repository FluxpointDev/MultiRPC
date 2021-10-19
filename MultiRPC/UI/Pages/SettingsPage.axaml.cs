using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MultiRPC.UI.Pages
{
    public partial class SettingsPage : SidePage
    {
        public SettingsPage() { }

        public override string IconLocation => "Icons/Settings";
        public override string LocalizableName => "Settings";
        public override void Initialize(bool loadXaml) => InitializeComponent(loadXaml);
    }
}