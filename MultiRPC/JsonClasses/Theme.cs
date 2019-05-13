using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;

namespace MultiRPC.JsonClasses
{
    public class Theme
    {
        static Theme()
        {
            if (!Directory.Exists(Path.Combine("Assets", "Themes")))
                Directory.CreateDirectory(Path.Combine("Assets", "Themes"));
        }

        public const string ThemeExtension = ".multirpctheme";

        public Color AccentColour1;
        public Color AccentColour2;
        public Color AccentColour2Hover;
        public Color AccentColour3;
        public Color AccentColour4;
        public Color AccentColour5;
        public Color TextColour;

        public SolidColorBrush DisabledButtonColour;
        public SolidColorBrush DisabledButtonTextColour;
        public SolidColorBrush NavButtonBackgroundSelected;
        public SolidColorBrush NavButtonIconColourSelected;

        public SolidColorBrush AccentColour1SCBrush => new SolidColorBrush(AccentColour1);
        public SolidColorBrush AccentColour2SCBrush => new SolidColorBrush(AccentColour2);
        public SolidColorBrush AccentColour2HoverSCBrush => new SolidColorBrush(AccentColour2Hover);
        public SolidColorBrush AccentColour3SCBrush => new SolidColorBrush(AccentColour3);
        public SolidColorBrush AccentColour4SCBrush => new SolidColorBrush(AccentColour4);
        public SolidColorBrush AccentColour5SCBrush => new SolidColorBrush(AccentColour5);
        public SolidColorBrush TextColourSCBrush => new SolidColorBrush(TextColour);

        public string ThemeName;
        /// <summary> Version of MultiRPC the theme was made for </summary>
        public Version MultiRPCVersion;

        public static Theme Dark = new Theme
        {
            AccentColour1 = Color.FromRgb(54,57,62),
            AccentColour2 = Color.FromRgb(44, 46, 48),
            AccentColour2Hover = Color.FromRgb(44, 42, 42),
            AccentColour3 = Color.FromRgb(255, 255, 255),
            AccentColour4 = Color.FromRgb(180, 180, 180),
            AccentColour5 = Color.FromRgb(112, 112, 122),
            TextColour = Color.FromRgb(255, 255, 255),

            DisabledButtonColour = new SolidColorBrush(Color.FromRgb(80,80,80)),
            DisabledButtonTextColour = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            NavButtonBackgroundSelected = new SolidColorBrush(Color.FromRgb(0, 171, 235)),
            NavButtonIconColourSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            ThemeName = "Dark",
            MultiRPCVersion = Assembly.GetExecutingAssembly().GetName().Version
        };

        public static Theme Light = new Theme
        {
            AccentColour1 = Color.FromRgb(255, 255, 255),
            AccentColour2 = Color.FromRgb(239, 242, 243),
            AccentColour2Hover = Color.FromRgb(234, 234, 234),
            AccentColour3 = Color.FromRgb(209, 209, 209),
            AccentColour4 = Color.FromRgb(180, 180, 180),
            AccentColour5 = Color.FromRgb(112, 112, 112),
            TextColour = Color.FromRgb(112, 112, 112),

            DisabledButtonColour = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            DisabledButtonTextColour = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            NavButtonBackgroundSelected = new SolidColorBrush(Color.FromRgb(0, 171, 235)),
            NavButtonIconColourSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            ThemeName = "Light",
            MultiRPCVersion = Assembly.GetExecutingAssembly().GetName().Version
        };

        public static Theme Russia = new Theme
        {
            AccentColour1 = Color.FromRgb(205, 0, 0),
            AccentColour2 = Color.FromRgb(214, 39, 24),
            AccentColour2Hover = Color.FromRgb(229, 54, 39),
            AccentColour3 = Color.FromRgb(255, 255, 255),
            AccentColour4 = Color.FromRgb(0, 54, 167),
            AccentColour5 = Color.FromRgb(0, 25, 76),
            TextColour = Color.FromRgb(255, 255, 255),

            DisabledButtonColour = new SolidColorBrush(Color.FromRgb(0, 54, 167)),
            DisabledButtonTextColour = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            NavButtonBackgroundSelected = new SolidColorBrush(Color.FromRgb(255, 216, 0)),
            NavButtonIconColourSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255)),

            ThemeName = "Russia",
            MultiRPCVersion = Assembly.GetExecutingAssembly().GetName().Version
        };

        public static Task<Theme> Load(string filepath)
        {
            var fileStream =
                File.OpenRead(filepath);
            Theme theme = new Theme();

            using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry readmeEntry = archive.GetEntry("colours.json");
                using (StreamReader writer = new StreamReader(readmeEntry.Open()))
                {
                    theme = JsonConvert.DeserializeObject<Theme>(writer.ReadToEnd());
                }
            }

            return Task.FromResult(theme);
        }

        public static string ThemeFileLocation(Theme theme)
        {
            return Path.Combine(FileLocations.ThemesFolder, theme.ThemeName + ThemeExtension);
        }

        public static ResourceDictionary ThemeToResourceDictionary(Theme theme)
        {
            ResourceDictionary themeDictionary = new ResourceDictionary();
            themeDictionary.Add("AccentColour1", theme.AccentColour1);
            themeDictionary.Add("AccentColour1SCBrush", theme.AccentColour1SCBrush);
            themeDictionary.Add("AccentColour2", theme.AccentColour2);
            themeDictionary.Add("AccentColour2SCBrush", theme.AccentColour2SCBrush);
            themeDictionary.Add("AccentColour2Hover", theme.AccentColour2Hover);
            themeDictionary.Add("AccentColour2HoverSCBrush", theme.AccentColour2HoverSCBrush);
            themeDictionary.Add("AccentColour3", theme.AccentColour3);
            themeDictionary.Add("AccentColour3SCBrush", theme.AccentColour3SCBrush);
            themeDictionary.Add("AccentColour4", theme.AccentColour4);
            themeDictionary.Add("AccentColour4SCBrush", theme.AccentColour4SCBrush);
            themeDictionary.Add("AccentColour5", theme.AccentColour5);
            themeDictionary.Add("AccentColour5SCBrush", theme.AccentColour5SCBrush);
            themeDictionary.Add("TextColour", theme.TextColour);
            themeDictionary.Add("TextColourSCBrush", theme.TextColourSCBrush);
            themeDictionary.Add("DisabledButtonColour", theme.DisabledButtonColour);
            themeDictionary.Add("DisabledButtonTextColour", theme.DisabledButtonTextColour);
            themeDictionary.Add("NavButtonBackgroundSelected", theme.NavButtonBackgroundSelected);
            themeDictionary.Add("NavButtonIconColourSelected", theme.NavButtonIconColourSelected);
            themeDictionary.Add("ThemeName", theme.ThemeName);
            themeDictionary.Add("MultiRPCVersion", theme.MultiRPCVersion);

            return themeDictionary;
        }

        public static Theme ResourceDictionaryToTheme(ResourceDictionary themeDictionary)
        {
            return new Theme
            {
                AccentColour1 = (Color)themeDictionary["AccentColour1"],
                AccentColour2 = (Color)themeDictionary["AccentColour2"],
                AccentColour2Hover = (Color)themeDictionary["AccentColour2Hover"],
                AccentColour3 = (Color)themeDictionary["AccentColour3"],
                AccentColour4 = (Color)themeDictionary["AccentColour4"],
                AccentColour5 = (Color)themeDictionary["AccentColour5"],
                TextColour = (Color)themeDictionary["TextColour"],
                DisabledButtonColour = (SolidColorBrush)themeDictionary["DisabledButtonColour"],
                DisabledButtonTextColour = (SolidColorBrush)themeDictionary["DisabledButtonTextColour"],
                NavButtonBackgroundSelected = (SolidColorBrush)themeDictionary["NavButtonBackgroundSelected"],
                NavButtonIconColourSelected = (SolidColorBrush)themeDictionary["NavButtonIconColourSelected"],
                ThemeName = themeDictionary["ThemeName"].ToString(),
                MultiRPCVersion = (Version)themeDictionary["MultiRPCVersion"]
            };
        }

        public static Task Save(Theme theme, string fileLocation = null)
        {
            if (string.IsNullOrWhiteSpace(fileLocation))
                fileLocation = ThemeFileLocation(theme);

            var fileStream =
                File.Create(fileLocation);

            using (fileStream)
            {
                using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry("colours.json");
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(theme));
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
