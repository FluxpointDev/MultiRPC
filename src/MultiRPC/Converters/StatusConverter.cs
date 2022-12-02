using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiRPC.Discord;

namespace MultiRPC.Converters;

public class StatusConverter : JsonConverter<DiscordStatus>
{
    public override DiscordStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var status = reader.GetString();
        return status switch
        {
            "operational" => DiscordStatus.Operational,
            "degraded_performance" => DiscordStatus.Degraded,
            "partial_outage" => DiscordStatus.PartialOutage,
            "major_outage" => DiscordStatus.MajorOutage,
            _ => throw new InvalidDataContractException()
        };
    }

    public override void Write(Utf8JsonWriter writer, DiscordStatus value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}