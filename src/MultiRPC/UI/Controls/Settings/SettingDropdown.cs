using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using MultiRPC.Exceptions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings.Attributes;
using TinyUpdate.Core.Extensions;

namespace MultiRPC.UI.Controls.Settings;

public class SettingDropdown<T> : SettingItem
{
    // ReSharper disable once StaticMemberInGenericType
    private static DataTemplate? _dataTemplate;
    private TextBlock _tblHeader;
    private ComboBox _cboSelection;
    public SettingDropdown()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
        InitializeComponent();
    }

    public SettingDropdown(Language header, IBaseSetting setting, MethodInfo getMethod, 
        MethodInfo setMethod, SettingSourceAttribute? sourceAttribute, LanguageSourceAttribute? languageSourceAttribute, bool isLocalizable)
        : base(header, setting, getMethod, setMethod)
    {
        InitializeComponent();
        header.TextObservable.Subscribe(x => _tblHeader.Text = x + ":");

        //We need a source attribute for this or we don't know where 
        if (sourceAttribute == null)
        {
            //TODO: Log
            return;
        }

        //Get values
        var valsMethod = setting.GetType().GetMethod(sourceAttribute.MethodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        var values = (T[]?)valsMethod?.Invoke(setting, Array.Empty<object>());
        if (values == null)
        {
            //TODO: Log
            return;
        }

        //Set what should be shown
        if (languageSourceAttribute == null)
        {
            _cboSelection.Items = isLocalizable ? values.Select(x => (Language)(x?.ToString() ?? "")) : values;
        }
        else
        {
            var languageMethod = setting.GetType().GetMethod(languageSourceAttribute.MethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            _cboSelection.Items = (Language[]?)languageMethod?.Invoke(setting, Array.Empty<object>());
        }
            
        //Set what the initial value is
        var currentVal = (T?)getMethod.Invoke(setting, null);
        _cboSelection.SelectedIndex = currentVal != null ? values.IndexOf(x => x?.Equals(currentVal) ?? false) : -1;
        _cboSelection.SelectionChanged += (sender, args) =>
        {
            var index = _cboSelection.SelectedIndex;
            if (index == -1)
            {
                return;
            }
                
            var newVal = values[index];
            if (newVal != null)
            {
                setMethod.Invoke(setting, new object[]{ newVal });
            }
            //TODO: Log that we didn't set
        };
    }

    [MemberNotNull(nameof(_tblHeader))]
    [MemberNotNull(nameof(_cboSelection))]
    [MemberNotNull(nameof(_dataTemplate))]
    private void InitializeComponent()
    {
        _dataTemplate ??= new DataTemplate
        {
            Content = new Func<IServiceProvider, object>(provider => new ControlTemplateResult(new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding("TextObservable^")
            }, this.FindNameScope())),
            DataType = typeof(Language)
        };
            
        _tblHeader = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        _cboSelection = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            ItemTemplate = _dataTemplate
        };
        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            Children =
            {
                _tblHeader,
                _cboSelection
            }
        };
    }
}