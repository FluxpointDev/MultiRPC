using System;
using System.Text.Json.Serialization;
using Fonderie;

namespace MultiRPC.Rpc;

public partial class RichPresence
{
    public RichPresence(string name, long id)
    {
        _name = name;
        ID = id;
    }

    [GeneratedProperty]
    private string _name;

    public long ID { get; set; }

    [JsonIgnore]
    public DiscordRPC.RichPresence Presence => Profile.ToRichPresence();
        
    public RpcProfile Profile { get; set; } = new RpcProfile();

    [GeneratedProperty]
    [JsonIgnore]
    private Uri? _customLargeImageUrl;

    [GeneratedProperty]
    [JsonIgnore]
    private Uri? _customSmallImageUrl;

    [GeneratedProperty] 
    private bool _useTimestamp;
}