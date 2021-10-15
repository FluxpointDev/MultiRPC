using MultiRPC.UI.Pages;

namespace MultiRPC.Rpc
{
    public abstract class RpcPage : SidePage
    {
        public abstract RichPresence RichPresence { get; protected set; }
    }
}