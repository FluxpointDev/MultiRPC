using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiRPC
{
    public static class Data
    {
        public static Dictionary<string, string> MultiRPCImages = MakeImagesDictionary().ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<Dictionary<string, string>> MakeImagesDictionary()
        {
            return new Dictionary<string, string>()
            {
                { App.Text.NoImage, "" },
                { "Discord", "https://i.imgur.com/QN5WA4W.png" },
                { "MultiRPC", "https://i.imgur.com/d6OLF2z.png" },
                { "Firefox", "https://i.imgur.com/oTuovMT.png" },
                { "Firefox Nightly", "https://i.imgur.com/JBjTLUs.png" },
                { "Google", "https://i.imgur.com/DJjs5Yc.png" },
                { "Mel", "https://i.imgur.com/SUm8SwK.png" },
                { "Youtube", "https://i.imgur.com/Hc9DirJ.png" },
                { "Kappa", "https://i.imgur.com/kdUCRrj.png" },
                { "Lol", "https://i.imgur.com/StXRONi.png" },
                { "Nyancat", "https://i.imgur.com/YoiJGh5.png" },
                { "Monstercat", "https://i.imgur.com/QTGPwi0.png" },
                { "Thonk", "https://i.imgur.com/P4Mvpmf.png" },
                { "Lul", "https://i.imgur.com/1Q1Nbin.png" },
                { "Pepe", "https://i.imgur.com/7ybyrw7.png" },
                { "Trollface", "https://i.imgur.com/tanLvrt.png" },
                { "Doge", "https://i.imgur.com/ytpmvjg.png" },
                { App.Text.Christmas, "https://i.imgur.com/NF2enEO.png" },
                { App.Text.Present, "https://i.imgur.com/qMfJKt6.png" },
                { "Neko", "https://i.imgur.com/l2RsYY7.png" },
                { App.Text.Popcorn, "https://i.imgur.com/xplfztu.png" },
                { "Skype", "https://i.imgur.com/PjQFB6d.png" },
                { App.Text.Games, "https://i.imgur.com/lPrT5BG.png" },
                { "Steam", "https://i.imgur.com/bKxJ7Lj.png" },
                { "Minecraft", "https://i.imgur.com/vnw6Z8X.png" },
                { "Coke", "https://i.imgur.com/GAsmn3P.png" }
            };
        }

        public static string GetImageValue(string ImageKey)
        {
            if (!string.IsNullOrWhiteSpace(ImageKey))
            {
                MultiRPCImages.TryGetValue(ImageKey, out string Uri);
                return Uri;
            }
            return "";
        }
    }
}
