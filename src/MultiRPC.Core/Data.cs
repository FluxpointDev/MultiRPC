using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace MultiRPC.Core
{
    /// <summary>
    /// Contains all the keys that the MultiRPC ApplicationID has
    /// </summary>
    public static class Data
    {
        static Data()
        {
            //TODO: Hook this up
            throw new NotImplementedException();
            //Settings.Current.LanguageChanged += (_, __) => MultiRPCImages = MakeImagesDictionary();
        }

        /// <summary>
        /// All the MultiRPC keys with a Uri to preview the image
        /// </summary>
        [NotNull]
        public static Dictionary<string, Uri> MultiRPCImages { get; private set; } = MakeImagesDictionary();

        private static Dictionary<string, Uri> MakeImagesDictionary() =>
        new Dictionary<string, Uri>
        {
            {LanguagePicker.GetLineFromLanguageFile("NoImage"), null},
            {"Discord", new Uri("https://i.imgur.com/QN5WA4W.png") },
            {"MultiRPC", new Uri("https://i.imgur.com/d6OLF2z.png") },
            {"Firefox", new Uri("https://i.imgur.com/oTuovMT.png") },
            {"Firefox Nightly", new Uri("https://i.imgur.com/JBjTLUs.png") },
            {"Google", new Uri("https://i.imgur.com/DJjs5Yc.png") },
            {"Mel", new Uri("https://i.imgur.com/SUm8SwK.png") },
            {"Youtube", new Uri("https://i.imgur.com/Hc9DirJ.png") },
            {"Kappa", new Uri("https://i.imgur.com/kdUCRrj.png") },
            {"MmLol", new Uri("https://i.imgur.com/StXRONi.png") },
            {"Nyancat", new Uri("https://i.imgur.com/YoiJGh5.png") },
            {"Monstercat", new Uri("https://i.imgur.com/QTGPwi0.png") },
            {"Thonk", new Uri("https://i.imgur.com/P4Mvpmf.png") },
            {"Lul", new Uri("https://i.imgur.com/1Q1Nbin.png") },
            {"Pepe", new Uri("https://i.imgur.com/7ybyrw7.png") },
            {"Trollface", new Uri("https://i.imgur.com/tanLvrt.png") },
            {"Doge", new Uri("https://i.imgur.com/ytpmvjg.png") },
            {LanguagePicker.GetLineFromLanguageFile("Christmas"), new Uri("https://i.imgur.com/NF2enEO.png") },
            {LanguagePicker.GetLineFromLanguageFile("Present"), new Uri("https://i.imgur.com/qMfJKt6.png") },
            {"Neko", new Uri("https://i.imgur.com/l2RsYY7.png") },
            {LanguagePicker.GetLineFromLanguageFile("Popcorn"), new Uri("https://i.imgur.com/xplfztu.png") },
            {"Skype", new Uri("https://i.imgur.com/PjQFB6d.png") },
            {LanguagePicker.GetLineFromLanguageFile("Games"), new Uri("https://i.imgur.com/lPrT5BG.png") },
            {"Steam", new Uri("https://i.imgur.com/bKxJ7Lj.png") },
            {"Minecraft", new Uri("https://i.imgur.com/vnw6Z8X.png") },
            {"Coke", new Uri("https://i.imgur.com/GAsmn3P.png") }
        };

        public static Uri GetImageValue(string imageKey)
        {
            if (string.IsNullOrWhiteSpace(imageKey))
            {
                return null;
            }

            MultiRPCImages.TryGetValue(imageKey, out var uri);
            return uri;
        }
    }
}