namespace MultiRPC.Programs
{
    public class Anime : IProgram
    {
        public Anime(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
        }
        public override void Update(DiscordRPC.RichPresence RP)
        {
            //DiscordRpc.UpdatePresence(RP);
        }
    }
}
