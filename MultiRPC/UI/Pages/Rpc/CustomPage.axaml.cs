using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages.Rpc
{
    public partial class CustomPage : RpcPage
    {
        public CustomPage() { }

        public override string IconLocation => "Icons/Custom";
        public override string LocalizableName => "Custom";
        public override RichPresence RichPresence { get; protected set; }
        
        public override void Initialize(bool loadXaml) => InitializeComponent(loadXaml);
    }
}