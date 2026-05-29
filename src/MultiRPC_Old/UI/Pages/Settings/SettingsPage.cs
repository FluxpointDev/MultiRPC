using System.Reflection;
using Avalonia;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings.Attributes;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Controls.Settings;
using Splat;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Pages.Settings;

public class SettingsPage : TabsPage, ISidePage
{
    private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(SettingsPage));
        
    public string IconLocation => "Icons/Settings";
    public string LocalizableName => "Settings";
    public Thickness ContentPadding { get; } = new Thickness(0);


    public override void Initialize(bool loadXaml)
    {
        AddTab(new AboutSettingsTab());
        foreach (IBaseSetting setting in Locator.Current.GetServices(typeof(IBaseSetting)))
        {
            SettingsTab? settingPage = null;
            SettingSourceAttribute? sourceAttribute = null;
            LanguageSourceAttribute? languageSourceAttribute = null;
            IsEditableAttribute? editAttribute = null;
                
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
                            name = nameAttribute.Name;
                            isSetting = true;
                            break;
                        case SettingSourceAttribute settingSourceAttribute:
                            sourceAttribute = settingSourceAttribute;
                            break;
                        case LanguageSourceAttribute settingLanguageSourceAttribute:
                            languageSourceAttribute = settingLanguageSourceAttribute;
                            break;
                        case IsEditableAttribute isEditableAttribute:
                            editAttribute = isEditableAttribute;
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

                var tabName = setting.GetType().GetProperty("Name", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
                settingPage ??= new SettingsTab
                {
                    TabName = tabName?.ToString(),
                    Margin = new Thickness(10)
                };
                
                //Get if the control can be edited
                var isEditable = true;
                if (editAttribute != null)
                {
                    var editMethod = !string.IsNullOrEmpty(editAttribute.MethodName) ? 
                        setting.GetType().GetMethod(editAttribute.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        : null;
                    isEditable = (bool?)editMethod?.Invoke(setting, Array.Empty<object>()) ?? editAttribute.IsEditable ?? true;
                }

                if (settingProperty.PropertyType.BaseType == typeof(Enum))
                {
                    var enumDropdown = new EnumDropdown(settingProperty.PropertyType, name, setting, getMethod, setMethod) { IsEnabled = isEditable };
                    settingPage.Add(enumDropdown);
                }
                else if (settingProperty.PropertyType == typeof(bool))
                {
                    var boolCheckbox = new BooleanCheckbox(name, setting, getMethod, setMethod) { IsEnabled = isEditable };
                    settingPage.Add(boolCheckbox);
                }
                else
                {
#pragma warning disable IL3050
                    var settingDropdownType = typeof(SettingDropdown<>).MakeGenericType(settingProperty.PropertyType);
#pragma warning restore IL3050
                    var settingDropdown = (SettingItem?)Activator.CreateInstance(settingDropdownType, name, setting, getMethod, 
                        setMethod, sourceAttribute, languageSourceAttribute, settingProperty.GetCustomAttribute<NoneLocalizableAttribute>() == null);

                    if (settingDropdown != null)
                    {
                        settingDropdown.IsEnabled = isEditable;
                        settingPage.Add(settingDropdown);
                        continue;
                    }
                    //TODO: Log
                }
            }

            if (settingPage != null)
            {
                AddTab(settingPage);
            }
        }

        base.Initialize(loadXaml);
    }
}