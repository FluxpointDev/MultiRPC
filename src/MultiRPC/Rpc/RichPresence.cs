using System;
using System.Text.Json.Serialization;
using Fonderie;
using MultiRPC.Discord;

namespace MultiRPC.Rpc;

public partial class RichPresence
{
    public RichPresence(string name, long id)
    {
        _name = name;
        _id = id;
        _lazyManager = new Lazy<ProfileAssetsManager>(() => ProfileAssetsManager.GetOrAddManager(_id));
    }

    [GeneratedProperty]
    private string _name;

    [GeneratedProperty, JsonPropertyName("ID")]
    private long _id;

    [GeneratedProperty] 
    private bool _useTimestamp;
    
    [JsonIgnore]
    public DiscordRPC.RichPresence Presence => Profile.ToRichPresence();
        
    public RpcProfile Profile { get; set; } = new RpcProfile();

    private Lazy<ProfileAssetsManager> _lazyManager;
    [JsonIgnore] 
    public ProfileAssetsManager AssetsManager => _lazyManager.Value;

    partial void OnIdChanged(long _, long value)
    {
        _lazyManager = new Lazy<ProfileAssetsManager>(() => ProfileAssetsManager.GetOrAddManager(value));
    }
}