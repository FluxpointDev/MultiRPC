using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI.Pages.Rpc
{
    public partial class MultiRpcPage : RpcPage
    {
        public override string IconLocation => "Icons/Discord";
        public override string LocalizableName => "MultiRPC";
        public override RichPresence RichPresence { get; protected set; } = SettingManager<MultiRPCSettings>.Setting.Presence;
        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            rpcControl.RichPresence = RichPresence;
            rpcControl.Initialize(loadXaml);
        }
    }
}