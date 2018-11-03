using MultiRPC.Programs;
using System.Collections.Generic;

namespace MultiRPC
{
    public enum WinType
    {
        Auto, Win10, Win8, Win7, WinXP
    }
    public static class Data
    {
        public static _Settings Settings = new _Settings();
        public static Dictionary<string, IProgram> Programs = new Dictionary<string, IProgram>();
        public static void Load()
        {
            Programs.Add("afk", new Afk("AFK", "469643793851744257", ""));
            Programs.Add("windows", new Windows("Windows", "469675182802599936", ""));
            Programs.Add("anime", new Anime("Anime", "451178426439565312", ""));
            Programs.Add("firefox", new Firefox("Firefox", "450894077165043722", "firefox"));
            Programs.Add("chrome", new Chrome("Chrome", "", "chrome"));
            Programs.Add("minecraft", new Minecraft("Minecraft", "", ""));
            Programs.Add("winmedia", new WindowsMediaPlayer("Win Media Player", "450910667331993601", ""));
            Programs.Add("custom", new Custom("Custom", "", ""));
        }
    }
}
