using System;
using MultiRPC.UI.Pages;

namespace MultiRPC.Rpc.Page
{
    public abstract class RpcPage : SidePage
    {
        public abstract RichPresence RichPresence { get; protected set; }

        public record CheckResult(bool Valid, string? ReasonWhy = null);

        public abstract bool PresenceValid { get; }
        public abstract event EventHandler? PresenceChanged;
        public abstract event EventHandler<bool> PresenceValidChanged;
    }
}