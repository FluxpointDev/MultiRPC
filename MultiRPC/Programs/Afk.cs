namespace MultiRPC.Programs
{
    public class Afk : IProgram
    {
        public Afk(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
        }

        public override void Update(DiscordRPC.RichPresence RP)
        {
            
        }
    }
}
