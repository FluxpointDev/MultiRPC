using System.Diagnostics;

namespace MultiRPC.Programs
{
    public class Firefox : IProgram
    {
        public Firefox(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
            Auto = true;
        }

        public override void Update(DiscordRPC.RichPresence RP)
        {
       //     RP.state = "";
            Process[] p = Process.GetProcessesByName("firefox");
        //    RP.largeImageKey = "firefoxnightly";
        //    RP.largeImageText = "Browsing...";
        //    RP.details = Proc.MainWindowTitle;
        //
        //    if (RP.details.Contains(" - Firefox Nightly"))
        //        RP.details = RP.details.Replace(" - Firefox Nightly", "");
        //    else
        //        RP.details = RP.details.Replace(" - Firefox", "");

            //int Count = 0;
      //     while (RP.details.StartsWith("("))
      //      {
       //         RP.details = RP.details.Replace($"({Count})", "");
       //         Count++;
       //     }

      //      Console.WriteLine(RP.details);
       //     if (RP.details.Contains(" - YouTube"))
         //   {
       //         RP.details = RP.details.Replace(" - YouTube", "");
        //        RP.smallImageKey = "youtube";
        //        RP.smallImageText = "YouTube";
       //         if (RP.details.Contains(" [Monstercat Release]"))
        //        {
       //             RP.details = RP.details.Replace(" [Monstercat Release]", "");
       //             RP.smallImageText = "YouTube - Monstercat";
       //         }
       //     }
       //     else if (RP.details == "Google" || RP.details.Contains("Google Search"))
        //    {
       //         RP.details = "Searching Google";
        //        RP.smallImageKey = "google";
       //         RP.smallImageText = "Never use bing";
        //    }
       //     else if (RP.details.EndsWith(" - Gmail"))
       //     {
       //         RP.details = "Gmail";
       //         RP.state = "Reading emails";
       //         RP.smallImageKey = "google";
       //         RP.smallImageText = "Never use bing";
       //     }
       //     else if (RP.details.StartsWith("Discord - "))
       //     {
       //         RP.state = RP.details.Replace("Discord - ", "");
       //         RP.details = "Discord";
       //         RP.smallImageKey = "discord";
       //         RP.smallImageText = "X days before outage";
       //     }
       //     else if (RP.details.EndsWith(" | Trello") && RP.details.Contains("("))
       //     {
       //         RP.state = RP.details.Replace(" | Trello", "");
       //
       //         RP.smallImageKey = "trello";
        //        RP.smallImageText = "Playing with chalk";
        //        if (RP.state == "Home" || RP.state == "Boards" || RP.state == "Profile" || RP.state == "Cards" || RP.state == "Settings" || RP.state == "Billing" || RP.state == "Keyboard Shortcuts")
        //        {
        //            RP.details = "Trello";
        //            RP.state = "Viewing profile";
        //        }
       //         else
        //        {
       //             RP.details = "- Trello Board -";
       //         }
       //     }

       //     if (RP.details.Length > 25)
       //     {
      //          IEnumerable<string> Strings = ChunksUpto(RP.details, 25);
      //          int Count2 = 0;
       //         foreach (string S in Strings)
        //        {
       //             if (Count2 == 0) RP.details = S;
       //             if (Count2 != 0) RP.state += S;
       //             Count2++;
       //         }
       //     }

      //      DiscordRpc.UpdatePresence(RP);
        }
    }
}
