using MultiRPC.Functions;
using MultiRPC.GUI;
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

        public static void AutoStart(MainWindow window)
        {
            if (App.Config.AutoStart == "MultiRPC")
            {
                window.ItemsAutoStart.SelectedIndex = 1;
                window.Menu.SelectedIndex = 0;
                window.BtnToggleRPC_Click(null, null);
            }
            else if (App.Config.AutoStart == "Custom")
            {
                RPC.Type = "custom";
                window.ItemsAutoStart.SelectedIndex = 2;
                window.Menu.SelectedIndex = 1;
                window.BtnToggleRPC_Click(null, null);
            }
        }

        public static void SetupCustom(MainWindow window)
        {
            if (App.Config.Disabled.ProgramsTab)
                window.TabPrograms.Width = 0;

            if (App.Config.Disabled.ProgramsTab)
            {
                App.WD.ToggleProgramsTab.IsChecked = true;
                FuncSettings.SetProgramsTab(window);
            }
            if (App.Config.Disabled.HelpIcons)
                App.WD.ToggleHelpIcons.IsChecked = true;
            if (App.Config.AFKTime)
                App.WD.ToggleAfkTime.IsChecked = true;
        }

        public static void SaveProfiles()
        {
            using (StreamWriter file = File.CreateText(App.ConfigFolder + "Profiles.json"))
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
            { "MultiRPC", "https://i.imgur.com/d6OLF2z.png" },
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
            { "Lul", "https://i.imgur.com/1Q1Nbin.png" },
            { "Pepe", "https://i.imgur.com/7ybyrw7.png" },
            { "Trollface", "https://i.imgur.com/tanLvrt.png" },
            { "Doge", "https://i.imgur.com/ytpmvjg.png" },
            { "Christmas", "https://i.imgur.com/NF2enEO.png" },
            { "Present", "https://i.imgur.com/qMfJKt6.png" },
            { "Neko", "https://i.imgur.com/l2RsYY7.png" },
            { "Popcorn", "https://i.imgur.com/xplfztu.png" },
            { "Skype", "https://i.imgur.com/PjQFB6d.png" },
            { "Games", "https://i.imgur.com/lPrT5BG.png" },
            { "Steam", "https://i.imgur.com/bKxJ7Lj.png" },
            { "Minecraft", "https://i.imgur.com/vnw6Z8X.png" },
            { "Coke", "https://i.imgur.com/GAsmn3P.png" }
        };
    }
}
