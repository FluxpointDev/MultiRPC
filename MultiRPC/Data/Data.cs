using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiRPC.Data
{
    public enum WinType
    {
        Auto, Win10, Win8, Win7, WinXP
    }
    public static class _Data
    {
        public static Dictionary<string, IProgram> Programs = new Dictionary<string, IProgram>();
        public static Dictionary<string, CustomProfile> Profiles = new Dictionary<string, CustomProfile>();


        public static void Load()
        {
            foreach(string s in MultiRPC_Images.Keys)
            {
                ComboBoxItem Box = new ComboBoxItem
                {
                    Content = s,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Background = new SolidColorBrush(Color.FromRgb(182, 182, 182))
            };
                ComboBoxItem Box2 = new ComboBoxItem
                {
                    Content = s,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Background = new SolidColorBrush(Color.FromRgb(182, 182, 182))
                };
                App.WD.ItemsDefaultLarge.Items.Add(Box);
                App.WD.ItemsDefaultSmall.Items.Add(Box2);
            }

            //Programs.Add("afk", new Afk("AFK", "469643793851744257", ""));
            //Programs.Add("windows", new Windows("Windows", "469675182802599936", ""));
            //Programs.Add("anime", new Anime("Anime", "451178426439565312", ""));
            //Programs.Add("firefox", new Firefox("Firefox", "450894077165043722", "firefox"));
            //Programs.Add("chrome", new Chrome("Chrome", "", "chrome"));
            //Programs.Add("minecraft", new Minecraft("Minecraft", "", ""));
            //Programs.Add("winmedia", new WindowsMediaPlayer("Win Media Player", "450910667331993601", ""));
            //Programs.Add("custom", new Custom("Custom", "", ""));
        }

        public static void SaveProfiles()
        {
            using (StreamWriter file = File.CreateText(RPC.ConfigFolder + "Profiles.json"))
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, Profiles);
            }
        }

        public static Dictionary<string, string> MultiRPC_Images = new Dictionary<string, string>()
        {
            { "Discord", "https://i.imgur.com/QN5WA4W.png" },
            { "MultiRPC", "https://i.imgur.com/VwBsOkG.png" },
            { "Firefox", "https://i.imgur.com/oTuovMT.png" },
            { "FirefoxNightly", "https://i.imgur.com/JBjTLUs.png" },
            { "Google", "https://i.imgur.com/DJjs5Yc.png" },
            { "Mel", "https://i.imgur.com/SUm8SwK.png" },
            { "Youtube", "https://i.imgur.com/Hc9DirJ.png" },
            { "Kappa", "https://i.imgur.com/kdUCRrj.png" },
            { "Mmlol", "https://i.imgur.com/StXRONi.png" },
            { "Nyancat", "https://i.imgur.com/YoiJGh5.png" },
            { "Monstercat", "https://i.imgur.com/QTGPwi0.png" },
            { "Thonk", "https://i.imgur.com/P4Mvpmf.png" },
            { "Lul", "https://i.imgur.com/ej6GQjc.png" },
            { "Pepe", "https://i.imgur.com/7ybyrw7.png" },
            { "Trollface", "https://i.imgur.com/tanLvrt.png" },
            { "Doge", "https://i.imgur.com/ytpmvjg.png" },
            { "Christmas", "https://i.imgur.com/NF2enEO.png" },
            { "Present", "https://i.imgur.com/qMfJKt6.png" }
        };
    }
}
