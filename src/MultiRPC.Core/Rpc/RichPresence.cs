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

        public DiscordRPC.RichPresence Presence { get; set; }
    }
}
