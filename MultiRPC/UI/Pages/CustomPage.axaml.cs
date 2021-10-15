using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages
{
    public partial class CustomPage : RpcPage
    {
        public CustomPage()
        {
            InitializeComponent();
        }

        public override string IconLocation => "Icons/Custom";
        public override string LocalizableName => "Custom";
        public override RichPresence RichPresence { get; protected set; }
    }
}