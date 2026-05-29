using System.Text.Json.Serialization;
using MultiRPC.Discord;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Rpc;

public partial class Presence : IEquatable<Presence>
{
    private Lazy<ProfileAssetsManager> _lazyManager;

    public Presence(string name, long id)
    {
        _name = name;
        _id = id;
        _lazyManager = new Lazy<ProfileAssetsManager>(() => ProfileAssetsManager.GetOrAddManager(_id));
    }

    [Notify]
    private string _name;

    [PropertyAttribute("[System.Text.Json.Serialization.JsonPropertyName(\"ID\")]")]
    [Notify]
    private long _id;
    
    [JsonIgnore]
    public DiscordRPC.RichPresence RichPresence => Profile.ToRichPresence();
        
    public RpcProfile Profile { get; set; } = new RpcProfile();

    [JsonIgnore] 
    public ProfileAssetsManager AssetsManager => _lazyManager.Value;

    private void OnIdChanged(long _, long value)
    {
        _lazyManager = new Lazy<ProfileAssetsManager>(() => ProfileAssetsManager.GetOrAddManager(value));
    }

    public bool Equals(Presence? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _name == other._name && _id == other._id && Profile.Equals(other.Profile);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Presence)obj);
    }

    public override int GetHashCode() => HashCode.Combine(_name, _id, Profile);
}