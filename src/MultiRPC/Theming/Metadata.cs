using System.Text.Json.Serialization;
using SemVersion;

namespace MultiRPC.Theming;

public class Metadata
{
    [JsonConstructor]
    public Metadata(string name, SemanticVersion version)
    {
        Version = version;
        Name = name;
    }

    [JsonPropertyName("MultiRPCVersion")]
    public SemanticVersion Version { get; set; }

    [JsonPropertyName("ThemeName")]
    public string Name { get; set; }

    public ThemeMode Mode { get; set; } = ThemeMode.Dark;
}