using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Fonderie;
using MultiRPC.Rpc;

namespace MultiRPC.Setting.Settings;

public partial class ProfilesSettings : BaseSetting
{
    public ProfilesSettings() 
        : this(new ObservableCollection<RichPresence>{ new RichPresence("Profile",  0) }) { }

    [JsonConstructor]
    public ProfilesSettings(ObservableCollection<RichPresence> profiles)
    {
        Profiles = profiles;
        foreach (var profile in Profiles)
        {
            profile.PropertyChanged += OnUpdate;
            profile.Profile.PropertyChanged += OnUpdate;
        }
        Profiles.CollectionChanged += (sender, args) =>
        {
            foreach (RichPresence profile in args.OldItems ?? Array.Empty<object>())
            {
                profile.PropertyChanged -= OnUpdate;
                profile.Profile.PropertyChanged -= OnUpdate;
            }
            foreach (RichPresence profile in args.NewItems ?? Array.Empty<object>())
            {
                profile.PropertyChanged += OnUpdate;
                profile.Profile.PropertyChanged += OnUpdate;
            }
            Save();
        };
    }

    private void OnUpdate(object? sender, PropertyChangedEventArgs args)
    {
        Save();
    }

    [GeneratedProperty] 
    private int _lastSelectedProfileIndex;
        
    public ObservableCollection<RichPresence> Profiles { get; }

    [JsonIgnore] 
    public override string Name => "Profiles";
        
    //TODO: Wait for PR to fix System.Text.Json source generator in DiscordRPC (PR to be made)
    public override JsonSerializerContext? SerializerContext { get; }
}