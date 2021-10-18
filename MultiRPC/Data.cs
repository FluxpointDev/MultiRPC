using System.Collections.Generic;

namespace MultiRPC
{
    public static class Data
    {
        public static Dictionary<string, string> MultiRPCImages = MakeImagesDictionary();

        public static Dictionary<string, string> MakeImagesDictionary()
        {
            return new Dictionary<string, string>
            {
                {Language.GetText("NoImage"), ""},
                {"Discord", "https://i.imgur.com/QN5WA4W.png"},
                {"MultiRPC", "https://i.imgur.com/d6OLF2z.png"},
                {"Firefox", "https://i.imgur.com/oTuovMT.png"},
                {"Firefox Nightly", "https://i.imgur.com/JBjTLUs.png"},
                {"Google", "https://i.imgur.com/DJjs5Yc.png"},
                {"Mel", "https://i.imgur.com/SUm8SwK.png"},
                {"Youtube", "https://i.imgur.com/Hc9DirJ.png"},
                {"Kappa", "https://i.imgur.com/kdUCRrj.png"},
                {"MmLol", "https://i.imgur.com/StXRONi.png"},
                {"Nyancat", "https://i.imgur.com/YoiJGh5.png"},
                {"Monstercat", "https://i.imgur.com/QTGPwi0.png"},
                {"Thonk", "https://i.imgur.com/P4Mvpmf.png"},
                {"Lul", "https://i.imgur.com/1Q1Nbin.png"},
                {"Pepe", "https://i.imgur.com/7ybyrw7.png"},
                {"Trollface", "https://i.imgur.com/tanLvrt.png"},
                {"Doge", "https://i.imgur.com/ytpmvjg.png"},
                {Language.GetText("Christmas"), "https://i.imgur.com/NF2enEO.png"},
                {Language.GetText("Present"), "https://i.imgur.com/qMfJKt6.png"},
                {"Neko", "https://i.imgur.com/l2RsYY7.png"},
                {Language.GetText("Popcorn"), "https://i.imgur.com/xplfztu.png"},
                {"Skype", "https://i.imgur.com/PjQFB6d.png"},
                {Language.GetText("Games"), "https://i.imgur.com/lPrT5BG.png"},
                {"Steam", "https://i.imgur.com/bKxJ7Lj.png"},
                {"Minecraft", "https://i.imgur.com/vnw6Z8X.png"},
                {"Coke", "https://i.imgur.com/GAsmn3P.png"}
            };
        }

        public static string? GetImageValue(string imageKey)
        {
            if (string.IsNullOrWhiteSpace(imageKey))
            {
                return string.Empty;
            }

            MultiRPCImages.TryGetValue(imageKey, out var uri);
            return uri;
        }
    }
}