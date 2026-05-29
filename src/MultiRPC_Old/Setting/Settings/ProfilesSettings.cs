using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MultiRPC.Rpc;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Setting.Settings;

public partial class ProfilesSettings : IBaseSetting<ProfilesSettings>
{
    public static string Name => "Profiles";

    public static JsonTypeInfo<ProfilesSettings> TypeInfo { get; } = ProfilesSettingsContext.Default.ProfilesSettings;

    public ProfilesSettings()
        : this(new ObservableCollection<Presence> { new Presence(Language.GetText(LanguageText.Profile), 0) }) { }

    [JsonConstructor]
    public ProfilesSettings(ObservableCollection<Presence> profiles)
    {
        Profiles = profiles;
        foreach (var profile in Profiles)
        {
            profile.PropertyChanged += OnUpdate;
            profile.Profile.PropertyChanged += OnUpdate;
        }

        Profiles.CollectionChanged += (sender, args) =>
        {
            foreach (Presence profile in args.OldItems ?? Array.Empty<object>())
            {
                profile.PropertyChanged -= OnUpdate;
                profile.Profile.PropertyChanged -= OnUpdate;
            }

            foreach (Presence profile in args.NewItems ?? Array.Empty<object>())
            {
                profile.PropertyChanged += OnUpdate;
                profile.Profile.PropertyChanged += OnUpdate;
            }

            ((IBaseSetting<ProfilesSettings>)this).Save();
        };
    }

    private void OnUpdate(object? sender, PropertyChangedEventArgs args) => ((IBaseSetting<ProfilesSettings>)this).Save();

    [Notify] private int _lastSelectedProfileIndex;

    public ObservableCollection<Presence> Profiles { get; }
}