using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Fonderie;
using MultiRPC.Setting.Settings.Attributes;

namespace MultiRPC.Setting.Settings
{
    public partial class GeneralSettings : Setting
    {
        [JsonIgnore]
        public override string Name => "General";

        [GeneratedProperty]
        private string? _lastUser = "NA#0000";

        [GeneratedProperty, SettingName("Client")]
        private DiscordClient _client;

        [GeneratedProperty, SettingName("AutoStart"), SettingSource(nameof(GetAutoStarts))]
        private string _autoStart = "";

        [GeneratedProperty, SettingName("Language"), SettingSource(nameof(GetLanguages)), NoneLocalizable]
        private string _language = "";
        
        [GeneratedProperty, SettingName("AfkTime")]
        private bool _showAfkTime;

        //TODO: Actually add logic which get real values
        private string[] GetLanguages()
        {
            return new[] { "e", "h" };
        }
        private string[] GetAutoStarts()
        {
            return new[] { "MultiRPC", "Custom" };
        }
    }
}