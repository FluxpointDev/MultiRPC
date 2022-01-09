using System.ComponentModel;
using System.Text.Json.Serialization;
using Fonderie;
using MultiRPC.Rpc;

namespace MultiRPC.Setting.Settings;

public partial class MultiRPCSettings : BaseSetting
{
    public MultiRPCSettings()
    {
        _presence.Profile.PropertyChanged += OnPropertyChanged;
        _presence.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(sender, args);
    }
        
    [GeneratedProperty]
    private RichPresence _presence = new RichPresence("MultiRPC", Constants.MultiRPCID)
    {
        Profile = new RpcProfile
        {
            LargeKey = "multirpc",
            Details = Language.GetText(LanguageText.Hello),
            State = Language.GetText(LanguageText.World)
        }
    };

    partial void OnPresenceChanged(RichPresence previous, RichPresence value)
    {
        previous.Profile.PropertyChanged -= OnPropertyChanged;
        previous.PropertyChanged -= OnPropertyChanged;

        value.Profile.PropertyChanged += OnPropertyChanged;
        value.PropertyChanged += OnPropertyChanged;
    }

    [JsonIgnore]
    public override string Name => "MultiRPC";

    //TODO: Wait for PR to fix System.Text.Json source generator in DiscordRPC (PR to be made)
    public override JsonSerializerContext? SerializerContext { get; }
}