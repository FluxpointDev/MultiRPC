using System.Text.Json.Serialization;
using MultiRPC.Discord;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Rpc;

public partial class RichPresence : IEquatable<RichPresence>
{
    public RichPresence(string name, long id)
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
    public DiscordRPC.RichPresence Presence => Profile.ToRichPresence();
        
    public RpcProfile Profile { get; set; } = new RpcProfile();

    private Lazy<ProfileAssetsManager> _lazyManager;
    [JsonIgnore] 
    public ProfileAssetsManager AssetsManager => _lazyManager.Value;

    private void OnIdChanged(long _, long value)
    {
        _lazyManager = new Lazy<ProfileAssetsManager>(() => ProfileAssetsManager.GetOrAddManager(value));
    }

    public bool Equals(RichPresence? other)
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
        return Equals((RichPresence)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_name, _id, Profile);
    }
}