using Avalonia.Controls;

namespace MultiRPC.Extensions;

public static class ControlExt
{
    public static void AddValidation(this Control control, Language? language,
        Action<string>? resultChanged, Func<string, CheckResult>? validation = null, Action<bool>? resultStatusChanged = null, string? initialValue = null)
    {
        var rpcControl = new ControlValidation(validation, initialValue) { Lang = language };
        control.DataContext = rpcControl;

        rpcControl.ResultChanged += (sender, s) => resultChanged?.Invoke(s);
        rpcControl.ResultStatusChanged += (sender, s) => resultStatusChanged?.Invoke(s);
    }
}