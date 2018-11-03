namespace MultiRPC.Programs
{
    public class Chrome : IProgram
    {
        public Chrome(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
            Auto = true;
        }

        public override void Update(DiscordRPC.RichPresence RP)
        {
           

            //DiscordRpc.UpdatePresence(RP);
        }
    }
}
