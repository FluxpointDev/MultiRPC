﻿using System;

namespace MultiRPC.Core.Rpc
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

        public DiscordRPC.RichPresence Presence { get; set; } = new DiscordRPC.RichPresence() 
        { Assets = new DiscordRPC.Assets() };

        public Uri CustomLargeImageUrl { get; set; }

        public Uri CustomSmallImageUrl { get; set; }

        public bool UseTimestamp { get; set; }

        public bool IsVaildPresence =>
            Presence.Details?.Length != 1
            && Presence.State?.Length != 1
            && Presence.Assets?.LargeImageText?.Length != 1
            && Presence.Assets?.SmallImageText?.Length != 1;
    }
}