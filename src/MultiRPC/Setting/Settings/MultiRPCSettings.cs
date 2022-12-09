using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MultiRPC.Rpc;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Setting.Settings;

public partial class MultiRPCSettings : IBaseSetting<MultiRPCSettings>
{
    public static string Name => "MultiRPC";

    public static JsonTypeInfo<MultiRPCSettings> TypeInfo { get; } = MultiRPCSettingsContext.Default.MultiRPCSettings;

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
    private Presence _presence = new Presence("MultiRPC", Constants.MultiRPCID)
    {
        Profile = new RpcProfile
        {
            LargeKey = "multirpc",
            Details = Language.GetText(LanguageText.Hello),
            State = Language.GetText(LanguageText.World)
        }
    };

    private void OnPresenceChanged(Presence previous, Presence value)
    {
        previous.Profile.PropertyChanged -= OnPropertyChanged;
        previous.PropertyChanged -= OnPropertyChanged;

        value.Profile.PropertyChanged += OnPropertyChanged;
        value.PropertyChanged += OnPropertyChanged;
    }
}