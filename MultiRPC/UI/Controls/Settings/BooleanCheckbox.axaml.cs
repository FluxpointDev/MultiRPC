using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MultiRPC.UI.Controls.Settings
{
    public partial class BooleanCheckbox : SettingItem
    {
        public BooleanCheckbox()
        {
            InitializeComponent();
        }
        
        public BooleanCheckbox(Language header, Setting.Setting setting, MethodInfo getMethod, MethodInfo setMethod)
            : base(header, setting, getMethod, setMethod)
        {
            InitializeComponent();
            
            var isChecked = getMethod.Invoke(setting, null);
            if (isChecked != null)
            {
                cboUI.IsChecked = (bool)isChecked;
            }

            cboUI.DataContext = header;
            cboUI.Checked += (sender, args) => setMethod.Invoke(setting, new object[]{ true });
            cboUI.Unchecked += (sender, args) => setMethod.Invoke(setting, new object[]{ false });
        }
    }
}