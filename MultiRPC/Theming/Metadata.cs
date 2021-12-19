using System;
using System.Text.Json.Serialization;

namespace MultiRPC.Theming;
public class Metadata
{
    [JsonConstructor]
    public Metadata(string name, Version version)
    {
        Version = version;
        Name = name;
    }

    [JsonPropertyName("MultiRPCVersion")]
    public Version Version { get; set; }

    [JsonPropertyName("ThemeName")]
    public string Name { get; set; }
}

[JsonSerializable(typeof(Metadata))]
public partial class MetadataContext : JsonSerializerContext { }