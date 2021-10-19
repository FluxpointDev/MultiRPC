using System;
using Avalonia.Controls;
using MultiRPC.UI.Pages;
using MultiRPC.UI.Pages.Rpc;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Rpc
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