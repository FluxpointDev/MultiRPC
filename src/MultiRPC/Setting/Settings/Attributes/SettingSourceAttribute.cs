namespace MultiRPC.Setting.Settings.Attributes;

/// <summary>
/// This tells us where to get all the values that we can switch too
/// </summary>
public class SettingSourceAttribute : Attribute
{
    public string MethodName { get; }
    public SettingSourceAttribute(string methodName) => MethodName = methodName;
}