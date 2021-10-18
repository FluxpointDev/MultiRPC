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

        protected void AddRpcControl(Control control, Language language, Action<string> a, Func<string, CheckResult>? validation = null)
        {
            var rpcControl = new RpcControlDataContext(validation) { Lang = language };
            control.DataContext = rpcControl;
            rpcControl.ResultChanged += (sender, s) =>
            {
                a.Invoke(s);
            };
        }
    }
}