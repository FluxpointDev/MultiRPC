using System.Reflection;
using MultiRPC.Setting;

namespace MultiRPC.UI.Controls.Settings
{
    public partial class BooleanCheckbox : SettingItem
    {
        public BooleanCheckbox()
        {
            InitializeComponent();
        }
        
        public BooleanCheckbox(Language header, BaseSetting setting, MethodInfo getMethod, MethodInfo setMethod)
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