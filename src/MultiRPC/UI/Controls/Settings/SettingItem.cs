using System.Reflection;
using Avalonia.Controls;
using MultiRPC.Exceptions;
using MultiRPC.Setting;

namespace MultiRPC.UI.Controls.Settings;

public class SettingItem : UserControl
{
    public SettingItem()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
    }

    public SettingItem(Language header, IBaseSetting setting, MethodInfo getMethod, MethodInfo setMethod)
    { }
}