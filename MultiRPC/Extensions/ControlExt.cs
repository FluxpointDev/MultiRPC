using System;
using Avalonia.Controls;
using MultiRPC.Rpc.Validation;

namespace MultiRPC.Extensions
{
    public static class ControlExt
    {
        public static void AddRpcControl(this Control control, Language language, Action<string> a, Func<string, CheckResult>? validation = null)
        {
            var rpcControl = new RpcControlValidation(validation) { Lang = language };
            control.DataContext = rpcControl;
            rpcControl.ResultChanged += (sender, s) =>
            {
                a.Invoke(s);
            };
        }
    }
}