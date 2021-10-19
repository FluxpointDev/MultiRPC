using System.ComponentModel;
using System.Text.Json.Serialization;
using Fonderie;

namespace MultiRPC.Setting.Settings
{
    public partial class GeneralSettings : Setting
    {
        [GeneratedProperty]
        private string? _lastUser = "NA#0000";

        [JsonIgnore]
        public override string Name => "General";
    }
}