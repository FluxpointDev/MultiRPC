namespace MultiRPC.Setting.Settings.Attributes;

/// <summary>
/// This tells us that the Property is a setting and what it's called
/// </summary>
public class SettingNameAttribute : Attribute
{
    public string Name { get; }

    public SettingNameAttribute(string name) => Name = name;
}