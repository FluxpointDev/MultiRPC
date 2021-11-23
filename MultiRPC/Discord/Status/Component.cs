using System;
using System.Text.Json.Serialization;

namespace MultiRPC.Discord.Status
{
    /*public class Page
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("time_zone")]
        public string TimeZone { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }*/

    public class Component
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("showcase")]
        public bool Showcase { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("group_id")]
        public string GroupId { get; set; }

        [JsonPropertyName("page_id")]
        public string PageId { get; set; }

        [JsonPropertyName("group")]
        public bool Group { get; set; }

        [JsonPropertyName("only_show_if_degraded")]
        public bool OnlyShowIfDegraded { get; set; }

        [JsonPropertyName("components")]
        public string[] Components { get; set; }
    }

    public class Data
    {
        //[JsonPropertyName("page")]
        //public Page Page { get; set; }

        [JsonPropertyName("components")]
        public Component[] Components { get; set; }
    }
}