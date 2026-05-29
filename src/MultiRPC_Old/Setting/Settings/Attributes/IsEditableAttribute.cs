namespace MultiRPC.Setting.Settings.Attributes;

public class IsEditableAttribute : Attribute
{
    public string? MethodName { get; }
    public bool? IsEditable { get; }
    public IsEditableAttribute(string methodName) => MethodName = methodName;
    public IsEditableAttribute(bool isEditable) => IsEditable = isEditable;
}