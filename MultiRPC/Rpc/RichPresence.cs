using System;
using DiscordRPC;

namespace MultiRPC.Rpc
{
    public class RichPresence
    {
        public RichPresence(string name, long id)
        {
            Name = name;
            ID = id;
        }

        public string Name { get; }

        public long ID { get; }

        public DiscordRPC.RichPresence Presence { get; set; } = new()
        {
            Assets = new DiscordRPC.Assets(),
            Buttons = new Button[] { new Button(), new Button() }
        };

        public Uri CustomLargeImageUrl { get; set; }

        public Uri CustomSmallImageUrl { get; set; }

        public bool UseTimestamp { get; set; }

        //TODO: Remake to use proper testing
        public bool IsValidPresence =>
            Presence.Details?.Length != 1
            && Presence.State?.Length != 1
            && Presence.Assets?.LargeImageText?.Length != 1
            && Presence.Assets?.SmallImageText?.Length != 1;
    }
}
