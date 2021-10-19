using MultiRPC.UI.Pages;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Rpc.Page
{
    public abstract class RpcPage : SidePage
    {
        protected ILogging Logger;
        protected RpcPage()
        {
            Logger = LoggingCreator.CreateLogger(GetType().Name);
        }
        
        public abstract RichPresence RichPresence { get; protected set; }

        public record CheckResult(bool Valid, string? ReasonWhy = null);
    }
}