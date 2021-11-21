using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Layout;
using MultiRPC.Setting.Settings;
using MultiRPC.Setting.Settings.Attributes;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Controls.Settings;
using Splat;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Pages.Settings
{
    public partial class SettingsPage : SidePage
    {
        private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(SettingsPage));
        
        public override string IconLocation => "Icons/Settings";
        public override string LocalizableName => "Settings";

        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);
            var tabPage = new TabsPage();
            tabPage.AddTabs(new AboutSettingsTab());
            
            foreach (var setting in Locator.Current.GetServices<Setting.Setting>())
            {
                SettingsTab? settingPage = null;
                SettingSourceAttribute? sourceAttribute = null;
                LanguageSourceAttribute? languageSourceAttribute = null;
                
                var type = setting.GetType();
                foreach (var settingProperty in type.GetProperties())
                {
                    var isSetting = false;
                    Language name = null!;
                    foreach (var attribute in settingProperty.GetCustomAttributes())
                    {
                        switch (attribute)
                        {
                            case SettingNameAttribute nameAttribute:
                                name = new Language(nameAttribute.Name);
                                isSetting = true;
                                break;
                            case SettingSourceAttribute settingSourceAttribute:
                                sourceAttribute = settingSourceAttribute;
                                break;
                            case LanguageSourceAttribute settingLanguageSourceAttribute:
                                languageSourceAttribute = settingLanguageSourceAttribute;
                                break;
                        }
                    }
                    if (!isSetting)
                    {
                        continue;
                    }

                    //Check that we can read + write to the property
                    MethodInfo? getMethod;
                    if (!settingProperty.CanRead || (getMethod = settingProperty.GetGetMethod()) == null)
                    {
                        _logger.Warning("Property {0} in {1} can't be read!", settingProperty.Name, type.Name);
                        continue;
                    }
                    MethodInfo? setMethod;
                    if (!settingProperty.CanWrite || (setMethod = settingProperty.GetSetMethod()) == null)
                    {
                        _logger.Warning("Property {0} in {1} can't be written too!", settingProperty.Name, type.Name);
                        continue;
                    }

                    settingPage ??= new SettingsTab { TabName = new Language(setting.Name) };
                    if (settingProperty.PropertyType.BaseType == typeof(Enum))
                    {
                        var enumDropdown = new EnumDropdown(settingProperty.PropertyType, name, setting, getMethod, setMethod);
                        settingPage.Add(enumDropdown);
                    }
                    else if (settingProperty.PropertyType == typeof(bool))
                    {
                        var boolCheckbox = new BooleanCheckbox(name, setting, getMethod, setMethod);
                        settingPage.Add(boolCheckbox);
                    }
                    else
                    {
                        var settingDropdownType = typeof(SettingDropdown<>).MakeGenericType(settingProperty.PropertyType);
                        var settingDropdown = (SettingItem)Activator.CreateInstance(settingDropdownType, new object[]
                        {
                            name, setting, getMethod, setMethod,
                            sourceAttribute, languageSourceAttribute,
                            settingProperty.GetCustomAttribute<NoneLocalizableAttribute>() == null
                        });
                        
                        settingPage.Add(settingDropdown);
                    }
                }

                if (settingPage != null)
                {
                    tabPage.AddTabs(settingPage);
                }
            }
            
            tabPage.Initialize();
            Content = tabPage;
        }
    }
}