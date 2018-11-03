namespace MultiRPC.Programs
{
    public class Custom : IProgram
    {
        public Custom(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
        }
        public string OverrideClient;
        public string Text1;
        public string Text2;
        public string TextLarge;
        public string ImgLarge;
        public string TextSmall;
        public string ImgSmall;

        public override void Update(DiscordRPC.RichPresence RP)
        {
           // DiscordRpc.UpdatePresence(RP);
          //  RP.details = Text1;
          //  RP.state = Text2;
          //  RP.largeImageKey = ImgLarge;
          //  RP.largeImageText = TextLarge;
          //  RP.smallImageKey = ImgSmall;
          //  RP.smallImageText = TextSmall;
        }
    }
}
