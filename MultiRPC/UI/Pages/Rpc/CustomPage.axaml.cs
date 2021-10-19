using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;

namespace MultiRPC.UI.Pages.Rpc
{
    //TODO: Add profiles
    public partial class CustomPage : RpcPage
    {
        public CustomPage() { }

        public override string IconLocation => "Icons/Custom";
        public override string LocalizableName => "Custom";
        public override RichPresence RichPresence { get; protected set; } = new RichPresence("", 0);
        
        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            rpcControl.RichPresence = RichPresence;
            rpcControl.Initialize(loadXaml);
        }
    }
}