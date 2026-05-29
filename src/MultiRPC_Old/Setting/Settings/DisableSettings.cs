using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MultiRPC.Setting.Settings.Attributes;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Setting.Settings;

public partial class DisableSettings : IBaseSetting<DisableSettings>
{
    public static string Name => "Disable";
    public static JsonTypeInfo<DisableSettings> TypeInfo { get; } = DisableSettingsContext.Default.DisableSettings;

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"DiscordCheck\")]")]
    [Notify]
    private bool _discordCheck;
        
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"TokenCheck\")]")]
    [Notify]
    private bool _tokenCheck;
    
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"HelpIcons\")]")]
    [Notify]
    private bool _helpIcons;
        
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"AutomaticUpdates\")]")]
    [Notify]
    private bool _autoUpdate = true;
        
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"HideTaskbarIcon\")]")]
    [Notify]
    private bool _hideTaskbarIcon;
        
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"ShowPageTooltips\")]")]
    [Notify]
    private bool _showPageTooltips;

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"ShowAllTooltips\")]")]
    [Notify]
    private bool _allTooltips;

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"AcrylicEffect\")]")]
    [PropertyAttribute($"[MultiRPC.Setting.Settings.Attributes.IsEditable(\"{nameof(CanEditAcrylicEffect)}\")]")]
    [Notify]
    private bool _acrylicEffect = !CanEditAcrylicEffect();
    
    [Notify]
    private bool _inviteWarn;

    [Notify]
    private bool _buttonWarn;

    private static bool CanEditAcrylicEffect() => !OperatingSystem.IsLinux();
}