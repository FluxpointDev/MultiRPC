using MultiRPC.UI.Controls.Settings;

namespace MultiRPC.Setting.Settings.Attributes;

/// <summary>
/// Tells the <see cref="SettingItem"/> where to find all the <see cref="Language"/>'s to show
/// </summary>
public class LanguageSourceAttribute : Attribute
{
    public string MethodName { get; }
    public LanguageSourceAttribute(string methodName) => MethodName = methodName;
}