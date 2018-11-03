using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MultiRPC.Programs
{
    public class Windows : IProgram
    {
        public Windows(string name, string client, string process)
        {
            Name = name;
            Client = client;
            ProcessName = process;
            Data = new ProgramData(Name);
        }
        public List<string> Strings = new List<string>
        {
            "Browsing files",
            "Deleting system32",
            "Emptying recycle bin",
            "Installing viruses",
            "Using internet explorer"
        };
        public WinType WinType = WinType.Win10;
        public override void Update(DiscordRPC.RichPresence RP)
        {
            WinType Type = WinType;
            if (WinType == WinType.Auto)
            {

                // Insert code to detect windows version
            }
            Type = WinType.Win10;
      //      RP.smallImageKey = "";
    //        RP.smallImageText = "";
     //       RP.startTimestamp = 0;
      //      RP.endTimestamp = 0;
      //      RP.largeImageText = "";
            switch (Type)
            {
                case WinType.Win10:
                    //            RP.details = "Windows 10";
                    //           RP.largeImageKey = "win8";
                    //          RP.largeImageText = "Inserting spyware...";
                    long ticks = Stopwatch.GetTimestamp();
                    double uptime = ((double)ticks) / Stopwatch.Frequency;
                    TimeSpan uptimeSpan = TimeSpan.FromSeconds(uptime);
                    //RP.state = $"{uptimeSpan.Hours} h {uptimeSpan.Minutes} m";
                    break;
                case WinType.Win8:
                    // Windows 8 icon
                    break;
                case WinType.Win7:
                    // Windows 7 icon
                    break;
                case WinType.WinXP:
                    // Windows XP icon
                    break;
            }
            // Pick a random string
         //   DiscordRpc.UpdatePresence(RP);
        }
    }
}
