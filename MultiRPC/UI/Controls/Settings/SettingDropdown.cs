using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings.Attributes;
using TinyUpdate.Core.Extensions;

namespace MultiRPC.UI.Controls.Settings
{
    //TODO: Sort out warnings
    public class SettingDropdown<T> : SettingItem
    {
        public SettingDropdown()
        {
            InitializeComponent();
        }
        
        public SettingDropdown(Language header, BaseSetting setting, MethodInfo getMethod, 
            MethodInfo setMethod, SettingSourceAttribute? sourceAttribute, LanguageSourceAttribute? languageSourceAttribute, bool isLocalizable)
            : base(header, setting, getMethod, setMethod)
        {
            InitializeComponent();
            header.TextObservable.Subscribe(x => tblHeader.Text = x + ":");

            //We need a source attribute for this or we don't know where 
            if (sourceAttribute == null)
            {
                //TODO: Log
                return;
            }

            //Get values
            var valsMethod = setting.GetType().GetMethod(sourceAttribute.MethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var values = (T[])valsMethod.Invoke(setting, Array.Empty<object>());

            //Set what should be shown
            if (languageSourceAttribute == null)
            {
                cboSelection.Items = isLocalizable ? values.Select(x => Language.GetLanguage(x?.ToString() ?? "")) : values;
            }
            else
            {
                var languageMethod = setting.GetType().GetMethod(languageSourceAttribute.MethodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                cboSelection.Items = (Language[])languageMethod.Invoke(setting, Array.Empty<object>());
            }
            
            //Set what the inital value is
            var currentVal = (T?)getMethod.Invoke(setting, null);
            cboSelection.SelectedIndex = currentVal != null ? values.IndexOf(x => x?.Equals(currentVal) ?? false) : -1;
            cboSelection.SelectionChanged += (sender, args) =>
            {
                var index = cboSelection.SelectedIndex;
                if (index == -1)
                {
                    return;
                }
                
                var newVal = values[index];
                setMethod.Invoke(setting, new object[]{ newVal });
            };
        }

        private static DataTemplate? _dataTemplate;
        
        public void InitializeComponent()
        {
            _dataTemplate ??= new DataTemplate
            {
                Content = new Func<IServiceProvider, object>(provider => new ControlTemplateResult(new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("TextObservable^")
                }, this.FindNameScope())),
                DataType = typeof(Language)
            };
            
            tblHeader = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            cboSelection = new ComboBox
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
                    tblHeader,
                    cboSelection
                }
            };
        }

        private TextBlock tblHeader;
        private ComboBox cboSelection;
    }
}