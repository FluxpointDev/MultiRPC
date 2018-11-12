using MultiRPC.Programs;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;

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
            foreach(string s in MultiRPC_Images.Keys)
            {
                ComboBoxItem Box = new ComboBoxItem
                {
                    Content = s,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(182, 182, 182))
            };
                ComboBoxItem Box2 = new ComboBoxItem
                {
                    Content = s,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(182, 182, 182))
                };
                MainWindow2.WD.Items_DefaultLarge.Items.Add(Box);
                MainWindow2.WD.Items_DefaultSmall.Items.Add(Box2);
            }

            Programs.Add("afk", new Afk("AFK", "469643793851744257", ""));
            Programs.Add("windows", new Windows("Windows", "469675182802599936", ""));
            Programs.Add("anime", new Anime("Anime", "451178426439565312", ""));
            Programs.Add("firefox", new Firefox("Firefox", "450894077165043722", "firefox"));
            Programs.Add("chrome", new Chrome("Chrome", "", "chrome"));
            Programs.Add("minecraft", new Minecraft("Minecraft", "", ""));
            Programs.Add("winmedia", new WindowsMediaPlayer("Win Media Player", "450910667331993601", ""));
            Programs.Add("custom", new Custom("Custom", "", ""));
        }

        public static Dictionary<string, string> MultiRPC_Images = new Dictionary<string, string>()
        {
            { "Discord", "https://i.imgur.com/QN5WA4W.png" },
            { "Firefox", "https://i.imgur.com/oTuovMT.png" },
            { "FirefoxNightly", "https://i.imgur.com/JBjTLUs.png" },
            { "Google", "https://i.imgur.com/DJjs5Yc.png" },
            { "Mel", "https://i.imgur.com/SUm8SwK.png" },
            { "MultiRPC", "https://i.imgur.com/sYBuOC2.png" },
            { "Youtube", "https://i.imgur.com/Hc9DirJ.png" },
            { "Kappa", "https://i.imgur.com/kdUCRrj.png" },
            { "Mmlol", "https://i.imgur.com/StXRONi.png" },
            { "Nyancat", "https://i.imgur.com/YoiJGh5.png" }
        };
    }
}
