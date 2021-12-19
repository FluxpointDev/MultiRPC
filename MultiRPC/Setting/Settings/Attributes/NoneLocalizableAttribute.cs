using System;

namespace MultiRPC.Setting.Settings.Attributes;

/// <summary>
/// Tells the <see cref="SettingItem{T}"/> that this can't be localized and should use the raw value
/// </summary>
public class NoneLocalizableAttribute : Attribute { }