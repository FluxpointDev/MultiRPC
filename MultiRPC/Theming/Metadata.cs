using System;
using System.Text.Json.Serialization;

namespace MultiRPC.Theming;
public class Metadata
{
    [JsonPropertyName("MultiRPCVersion")]
    public Version Version { get; init; }

    [JsonPropertyName("ThemeName")]
    public string Name { get; init; }
}