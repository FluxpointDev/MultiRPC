using System.Reflection;
using Avalonia.Controls;
using Avalonia.Data;
using MultiRPC.Setting;
using Avalonia;
using MultiRPC.Exceptions;

namespace MultiRPC.UI.Controls.Settings;

public class BooleanCheckbox : SettingItem
{
    public BooleanCheckbox()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
        //TODO: Add
        //InitializeComponent(Language.GetLanguage("Test"), TestSetting.STestSetting, );
    }
    
    public BooleanCheckbox(Language header, IBaseSetting setting, MethodInfo getMethod, MethodInfo setMethod)
        : base(header, setting, getMethod, setMethod)
    {
        InitializeComponent(header, setting, getMethod, setMethod);
    }

    private void InitializeComponent(Language header, IBaseSetting setting, MethodInfo getMethod, MethodInfo setMethod)
    {
        var cboUI = new CheckBox();
        var binding = new Binding
        {
            Source = header,
            Path = "TextObservable^"
        };
        cboUI.Bind(ContentProperty, binding);
        Content = cboUI;

        var isChecked = getMethod.Invoke(setting, null);
        if (isChecked != null)
        {
            cboUI.IsChecked = (bool)isChecked;
        }

        cboUI.Checked += (sender, args) => setMethod.Invoke(setting, new object[]{ true });
        cboUI.Unchecked += (sender, args) => setMethod.Invoke(setting, new object[]{ false });
    }
}