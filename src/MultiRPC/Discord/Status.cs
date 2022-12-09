// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618
using System.Text.Json.Serialization;
using MultiRPC.Converters;

namespace MultiRPC.Discord;

/*public class Page
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("url")]
    public string Url { get; init; }

    [JsonPropertyName("time_zone")]
    public string TimeZone { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
}*/

public class Component
{
    /*[JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }*/

    [JsonPropertyName("status"), JsonConverter(typeof(StatusConverter))]
    public DiscordStatus Status { get; set; }

    /*[JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    [JsonPropertyName("position")]
    public int Position { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("showcase")]
    public bool Showcase { get; init; }

    [JsonPropertyName("start_date")]
    public string StartDate { get; init; }

    [JsonPropertyName("group_id")]
    public string GroupId { get; init; }

    [JsonPropertyName("page_id")]
    public string PageId { get; init; }

    [JsonPropertyName("group")]
    public bool Group { get; init; }

    [JsonPropertyName("only_show_if_degraded")]
    public bool OnlyShowIfDegraded { get; init; }

    [JsonPropertyName("components")]
    public string[] Components { get; init; }*/
}

public class Status
{
    //[JsonPropertyName("page")]
    //public Page Page { get; init; }

    [JsonPropertyName("components")]
    public Component[] Components { get; set; }
}