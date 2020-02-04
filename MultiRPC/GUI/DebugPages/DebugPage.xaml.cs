using MultiRPC.Core.Enums;

namespace MultiRPC.GUI.DebugPages
{
    /// <summary>
    /// Interaction logic for DebugPage.xaml
    /// </summary>
    public partial class DebugPage : PageWithIcon
    {
        public override MultiRPCIcons IconName => MultiRPCIcons.Debug;
        public override string JsonContent => "Debug";

        public DebugPage()
        {
            InitializeComponent();
        }
    }
}
