using System.ComponentModel;
using System.Text.Json.Serialization;
using MultiRPC.Rpc;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Setting.Settings;

public partial class MultiRPCSettings : BaseSetting
{
    [JsonIgnore]
    public override string Name => "MultiRPC";

    public MultiRPCSettings()
    {
        _presence.Profile.PropertyChanged += OnPropertyChanged;
        _presence.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(sender, args);
    }
        
    [Notify]
    private RichPresence _presence = new RichPresence("MultiRPC", Constants.MultiRPCID)
    {
        Profile = new RpcProfile
        {
            LargeKey = "multirpc",
            Details = Language.GetText(LanguageText.Hello),
            State = Language.GetText(LanguageText.World)
        }
    };

    private void OnPresenceChanged(RichPresence previous, RichPresence value)
    {
        previous.Profile.PropertyChanged -= OnPropertyChanged;
        previous.PropertyChanged -= OnPropertyChanged;

        value.Profile.PropertyChanged += OnPropertyChanged;
        value.PropertyChanged += OnPropertyChanged;
    }
}