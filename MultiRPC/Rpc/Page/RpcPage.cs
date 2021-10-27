using MultiRPC.UI.Pages;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Rpc.Page
{
    public abstract class RpcPage : SidePage
    {
        public abstract RichPresence RichPresence { get; protected set; }

        public record CheckResult(bool Valid, string? ReasonWhy = null);
    }
}