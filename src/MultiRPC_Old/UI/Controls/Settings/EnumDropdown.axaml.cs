using System.Reflection;
using MultiRPC.Setting;
using TinyUpdate.Core.Extensions;
using Avalonia.Controls;
using Avalonia.Layout;
using MultiRPC.Exceptions;

namespace MultiRPC.UI.Controls.Settings;

public class EnumDropdown : SettingItem
{
    public EnumDropdown()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
        //TODO: Add
        //InitializeComponent(Language.GetLanguage("Test"), TestSetting.STestSetting, );
    }
    
    public EnumDropdown(Type enumType, Language header, IBaseSetting setting, MethodInfo getMethod, MethodInfo setMethod)
        : base(header, setting, getMethod, setMethod)
    {
        InitializeComponent(enumType, header, setting, getMethod, setMethod);
    }
    
    private void InitializeComponent(Type enumType, Language header, IBaseSetting setting, MethodBase getMethod, MethodBase setMethod)
    {
        var tblHeader = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        var cboSelection = new ComboBox { VerticalAlignment = VerticalAlignment.Center };
        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            Children = { tblHeader, cboSelection }
        };
        
        header.TextObservable.Subscribe(x => tblHeader.Text = x + ":");
        var names = Enum.GetNames(enumType);
        cboSelection.Items = names.Select(Language.GetLanguage);
            
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