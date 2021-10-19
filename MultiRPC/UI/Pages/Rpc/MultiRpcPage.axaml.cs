using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;

namespace MultiRPC.UI.Pages.Rpc
{
    public partial class MultiRpcPage : RpcPage
    {
        public MultiRpcPage() { }

        public override string IconLocation => "Icons/Discord";
        public override string LocalizableName => "MultiRPC";
        public override RichPresence RichPresence { get; protected set; } = new RichPresence("MultiRPC", Constants.MultiRPCID);
        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            rpcControl.RichPresence = RichPresence;
            rpcControl.Initialize(loadXaml);
        }
    }
}