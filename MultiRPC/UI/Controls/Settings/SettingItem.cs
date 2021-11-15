using System;
using System.Reflection;
using Avalonia.Controls;
using MultiRPC.Exceptions;

namespace MultiRPC.UI.Controls.Settings
{
    public class SettingItem : UserControl
    {
        public SettingItem()
        {
            if (!Design.IsDesignMode)
            {
                throw new DesignException();
            }
        }

        public SettingItem(Language header, Setting.Setting setting, MethodInfo getMethod, MethodInfo setMethod)
        { }
    }
}