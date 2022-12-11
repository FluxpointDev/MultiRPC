using System.Text.Json.Serialization;
using MultiRPC.Discord;
using MultiRPC.Legacy;
using MultiRPC.Rpc;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;

namespace MultiRPC;

#pragma warning disable SYSLIB0020

//TODO: Find a way which will let us be able to use Metadata as well on these which have Serialization (Trying to access state and details)
[JsonSerializable(typeof(Presence))]
public partial class RichPresenceContext : JsonSerializerContext { }

[JsonSerializable(typeof(ProfilesSettings))]
public partial class ProfilesSettingsContext : JsonSerializerContext { }

[JsonSerializable(typeof(MultiRPCSettings))]
public partial class MultiRPCSettingsContext : JsonSerializerContext { }

[JsonSerializable(typeof(DisableSettings))]
public partial class DisableSettingsContext : JsonSerializerContext { }

[JsonSerializable(typeof(GeneralSettings))]
public partial class GeneralSettingsContext : JsonSerializerContext { }

[JsonSerializable(typeof(Dictionary<string, CustomProfile>), TypeInfoPropertyName = "OldProfiles")]
public partial class OldProfilesContext : JsonSerializerContext { }

[JsonSerializable(typeof(Config))]
public partial class ConfigContext : JsonSerializerContext { }

[JsonSerializable(typeof(Dictionary<MultiRPC.LanguageText, string>), GenerationMode = JsonSourceGenerationMode.Metadata, TypeInfoPropertyName = "Languages")]
public partial class LanguageFileContext : JsonSerializerContext { }

[JsonSerializable(typeof(CreditsList))]
public partial class CreditsListContext : JsonSerializerContext { }

[JsonSerializable(typeof(ClientCheckResult))]
public partial class ClientCheckResultContext : JsonSerializerContext { }

[JsonSerializable(typeof(DiscordAsset[]))]
public partial class DiscordAssetContext : JsonSerializerContext { }

[JsonSerializable(typeof(Status))]
public partial class StatusContext : JsonSerializerContext { }

[JsonSerializable(typeof(Colours))]
public partial class ColoursContext : JsonSerializerContext { }

[JsonSerializable(typeof(Metadata))]
public partial class MetadataContext : JsonSerializerContext { }

#pragma warning restore SYSLIB0020