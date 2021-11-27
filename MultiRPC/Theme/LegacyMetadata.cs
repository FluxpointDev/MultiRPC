using System;
using System.Text.Json.Serialization;

namespace MultiRPC.Theme;
public class LegacyMetadata
{
    [JsonPropertyName("MultiRPCVersion")]
    public Version Version { get; init; }

    [JsonPropertyName("ThemeName")]
    public string Name { get; init; }
}