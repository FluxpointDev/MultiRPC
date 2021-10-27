using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MultiRPC.UI.Pages
{
    public partial class ThemeEditorPage : SidePage
    {
        public override string IconLocation => "Icons/ThemeEditor";
        public override string LocalizableName => "ThemeEditor";
        public override void Initialize(bool loadXaml) => InitializeComponent(loadXaml);
    }
}