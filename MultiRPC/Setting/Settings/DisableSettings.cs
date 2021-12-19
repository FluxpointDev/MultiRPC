using System.Text.Json.Serialization;
using Fonderie;
using MultiRPC.Setting.Settings.Attributes;

namespace MultiRPC.Setting.Settings
{
    public partial class DisableSettings : BaseSetting
    {
        [JsonIgnore]
        public override string Name => "Disable";

        public override JsonSerializerContext? SerializerContext { get; } = DisableSettingsContext.Default;

        [GeneratedProperty, SettingName("DiscordCheck")]
        private bool _discordCheck;
        
        [GeneratedProperty, SettingName("TokenCheck")]
        private bool _tokenCheck;
        
        [GeneratedProperty, SettingName("HelpIcons")]
        private bool _helpIcons;
        
        [GeneratedProperty, SettingName("AutomaticUpdates")]
        private bool _autoUpdate = true;
        
        [GeneratedProperty, SettingName("HideTaskbarIcon")]
        private bool _hideTaskbarIcon;
        
        [GeneratedProperty, SettingName("ShowPageTooltips")]
        private bool _showPageTooltips;

        [GeneratedProperty]
        private bool _inviteWarn;
    }
            
    [JsonSerializable(typeof(DisableSettings))]
    public partial class DisableSettingsContext : JsonSerializerContext { }
}