using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MultiRPC.UI.Pages
{
    public partial class CreditsPage : SidePage
    {
        public override string IconLocation => "Icons/Credits";
        public override string LocalizableName => "Credits";
        public override void Initialize(bool loadXaml) => InitializeComponent(loadXaml);
    }
}