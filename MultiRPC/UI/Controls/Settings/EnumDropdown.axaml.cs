using System;
using System.Linq;
using System.Reflection;
using MultiRPC.Setting;
using TinyUpdate.Core.Extensions;

namespace MultiRPC.UI.Controls.Settings
{
    public partial class EnumDropdown : SettingItem
    {
        public EnumDropdown()
        {
            InitializeComponent();
        }
        
        public EnumDropdown(Type enumType, Language header, BaseSetting setting, MethodInfo getMethod, MethodInfo setMethod)
            : base(header, setting, getMethod, setMethod)
        {
            InitializeComponent();

            header.TextObservable.Subscribe(x => tblHeader.Text = x + ":");
            var names = Enum.GetNames(enumType);
            cboSelection.Items = names.Select(x => new Language(x));
            
            var n = getMethod.Invoke(setting, null)?.ToString();
            cboSelection.SelectedIndex = n != null ? names.IndexOf(x => x == n) : -1;
            
            cboSelection.SelectionChanged += (sender, args) =>
            {
                var index = cboSelection.SelectedIndex;
                if (index == -1)
                {
                    return;
                }
                
                var newEnum = Enum.Parse(enumType, names[index]);
                setMethod.Invoke(setting, new []{ newEnum });
            };
        }
    }
}