using System.ComponentModel;
using System.Text.Json.Serialization;
using Fonderie;

namespace MultiRPC.Setting.Settings
{
    public partial class GeneralSettings : Setting
    {
        [JsonIgnore]
        public override string Name => "General";

        [GeneratedProperty]
        private string? _lastUser = "NA#0000";

        [GeneratedProperty]
        private bool _showAfkTime = false;
    }
}