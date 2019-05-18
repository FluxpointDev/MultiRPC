using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;

namespace MultiRPC.JsonClasses
{
    public class Theme
    {
        public const string ThemeExtension = ".multirpctheme";

        public static Theme Dark = new Theme
        {
            ThemeColours = new Colours
            {
                AccentColour1 = Color.FromRgb(54, 57, 62),
                AccentColour2 = Color.FromRgb(44, 46, 48),
                AccentColour2Hover = Color.FromRgb(44, 42, 42),
                AccentColour3 = Color.FromRgb(255, 255, 255),
                AccentColour4 = Color.FromRgb(180, 180, 180),
                AccentColour5 = Color.FromRgb(112, 112, 122),
                TextColour = Color.FromRgb(255, 255, 255),

                DisabledButtonColour = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                DisabledButtonTextColour = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                NavButtonBackgroundSelected = new SolidColorBrush(Color.FromRgb(0, 171, 235)),
                NavButtonIconColourSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255))
            },
            ThemeMetadata = new Metadata
            {
                ThemeName = "Dark",
                MultiRPCVersion = Assembly.GetExecutingAssembly().GetName().Version
            }
        };

        public static Theme Light = new Theme
        {
            ThemeColours = new Colours
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
                NavButtonIconColourSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255))
            },
            ThemeMetadata = new Metadata
            {
                ThemeName = "Light",
                MultiRPCVersion = Assembly.GetExecutingAssembly().GetName().Version
            }
        };

        public static Theme Russia = new Theme
        {
            ThemeColours = new Colours
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
                NavButtonIconColourSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255))
            },
            ThemeMetadata = new Metadata
            {
                ThemeName = "Russia",
                MultiRPCVersion = Assembly.GetExecutingAssembly().GetName().Version
            }
        };

        public Colours ThemeColours;
        public Metadata ThemeMetadata;

        static Theme()
        {
            if (!Directory.Exists(Path.Combine("Assets", "Themes")))
                Directory.CreateDirectory(Path.Combine("Assets", "Themes"));
        }

        public static Task<Theme> Load(string filepath)
        {
            var theme = new Theme();
            if (File.Exists(filepath))
            {
                FileStream fileStream;
                try
                {
                    fileStream =
                        File.OpenRead(filepath);
                }
                catch
                {
                    App.Logging.Application(App.Text.CantOpenOrSaveThemeFile.Replace("{open/save}", App.Text.Open));
                    return Task.FromResult(theme);
                }

                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    var coloursEntry = archive.GetEntry("colours.json");
                    var metadataEntry = archive.GetEntry("metadataEntry.json");
                    try
                    {
                        using (var writer = new StreamReader(coloursEntry.Open()))
                        {
                            theme.ThemeColours = JsonConvert.DeserializeObject<Colours>(writer.ReadToEnd());
                        }
                    }
                    catch
                    {
                        App.Logging.Application(
                            $"{App.Text.SomethingHappenedWhile} {App.Text.Getting} {filepath} {App.Text.ThemeColours}");
                    }

                    try
                    {
                        using (var writer = new StreamReader(metadataEntry.Open()))
                        {
                            theme.ThemeMetadata = JsonConvert.DeserializeObject<Metadata>(writer.ReadToEnd());
                        }
                    }
                    catch
                    {
                        App.Logging.Application(
                            $"{App.Text.SomethingHappenedWhile} {App.Text.Getting} {filepath} {App.Text.ThemeMetadata}");
                    }
                }
            }
            else
            {
                App.Logging.Application($"{filepath} {App.Text.DoesntExist}!!!");
            }


            return Task.FromResult(theme);
        }

        public static string ThemeFileLocation(Theme theme)
        {
            return Path.Combine(FileLocations.ThemesFolder, theme.ThemeMetadata.ThemeName + ThemeExtension);
        }

        public static ResourceDictionary ThemeToResourceDictionary(Theme theme)
        {
            var themeDictionary = new ResourceDictionary
            {
                {"AccentColour1", theme.ThemeColours.AccentColour1},
                {"AccentColour1SCBrush", theme.ThemeColours.AccentColour1SCBrush},
                {"AccentColour2", theme.ThemeColours.AccentColour2},
                {"AccentColour2SCBrush", theme.ThemeColours.AccentColour2SCBrush},
                {"AccentColour2Hover", theme.ThemeColours.AccentColour2Hover},
                {"AccentColour2HoverSCBrush", theme.ThemeColours.AccentColour2HoverSCBrush},
                {"AccentColour3", theme.ThemeColours.AccentColour3},
                {"AccentColour3SCBrush", theme.ThemeColours.AccentColour3SCBrush},
                {"AccentColour4", theme.ThemeColours.AccentColour4},
                {"AccentColour4SCBrush", theme.ThemeColours.AccentColour4SCBrush},
                {"AccentColour5", theme.ThemeColours.AccentColour5},
                {"AccentColour5SCBrush", theme.ThemeColours.AccentColour5SCBrush},
                {"TextColour", theme.ThemeColours.TextColour},
                {"TextColourSCBrush", theme.ThemeColours.TextColourSCBrush},
                {"DisabledButtonColour", theme.ThemeColours.DisabledButtonColour},
                {"DisabledButtonTextColour", theme.ThemeColours.DisabledButtonTextColour},
                {"NavButtonBackgroundSelected", theme.ThemeColours.NavButtonBackgroundSelected},
                {"NavButtonIconColourSelected", theme.ThemeColours.NavButtonIconColourSelected},

                {"ThemeName", theme.ThemeMetadata.ThemeName},
                {"MultiRPCVersion", theme.ThemeMetadata.MultiRPCVersion}
            };

            return themeDictionary;
        }

        public static Theme ResourceDictionaryToTheme(ResourceDictionary themeDictionary)
        {
            return new Theme
            {
                ThemeColours = new Colours
                {
                    AccentColour1 = (Color) themeDictionary["AccentColour1"],
                    AccentColour2 = (Color) themeDictionary["AccentColour2"],
                    AccentColour2Hover = (Color) themeDictionary["AccentColour2Hover"],
                    AccentColour3 = (Color) themeDictionary["AccentColour3"],
                    AccentColour4 = (Color) themeDictionary["AccentColour4"],
                    AccentColour5 = (Color) themeDictionary["AccentColour5"],
                    TextColour = (Color) themeDictionary["TextColour"],
                    DisabledButtonColour = (SolidColorBrush) themeDictionary["DisabledButtonColour"],
                    DisabledButtonTextColour = (SolidColorBrush) themeDictionary["DisabledButtonTextColour"],
                    NavButtonBackgroundSelected = (SolidColorBrush) themeDictionary["NavButtonBackgroundSelected"],
                    NavButtonIconColourSelected = (SolidColorBrush) themeDictionary["NavButtonIconColourSelected"]
                },
                ThemeMetadata = new Metadata
                {
                    ThemeName = themeDictionary["ThemeName"].ToString(),
                    MultiRPCVersion = (Version) themeDictionary["MultiRPCVersion"]
                }
            };
        }

        public static Task Save(Theme theme, string fileLocation = null)
        {
            if (string.IsNullOrWhiteSpace(fileLocation))
                fileLocation = ThemeFileLocation(theme);

            FileStream fileStream;
            try
            {
                fileStream =
                    File.Create(fileLocation);
            }
            catch
            {
                App.Logging.Application(App.Text.CantOpenOrSaveThemeFile.Replace("{open/save}", App.Text.Save));
                return Task.FromResult(theme);
            }

            using (fileStream)
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    var coloursEntry = archive.CreateEntry("colours.json");
                    try
                    {
                        using (var writer = new StreamWriter(coloursEntry.Open()))
                        {
                            App.JsonSerializer.Serialize(writer, theme.ThemeColours);
                        }
                    }
                    catch
                    {
                        App.Logging.Application(
                            $"{App.Text.SomethingHappenedWhile} {App.Text.Saving} {fileLocation} {App.Text.ThemeColours}");
                    }

                    var metadataEntry = archive.CreateEntry("metadataEntry.json");
                    try
                    {
                        using (var writer = new StreamWriter(metadataEntry.Open()))
                        {
                            App.JsonSerializer.Serialize(writer, theme.ThemeMetadata);
                        }
                    }
                    catch
                    {
                        App.Logging.Application(
                            $"{App.Text.SomethingHappenedWhile} {App.Text.Saving} {fileLocation} {App.Text.ThemeMetadata}");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public class Colours
        {
            public Color AccentColour1;
            public Color AccentColour2;
            public Color AccentColour2Hover;
            public Color AccentColour3;
            public Color AccentColour4;
            public Color AccentColour5;

            public SolidColorBrush DisabledButtonColour;
            public SolidColorBrush DisabledButtonTextColour;

            public SolidColorBrush NavButtonBackgroundSelected;
            public SolidColorBrush NavButtonIconColourSelected;
            public Color TextColour;

            public SolidColorBrush AccentColour1SCBrush => new SolidColorBrush(AccentColour1);
            public SolidColorBrush AccentColour2SCBrush => new SolidColorBrush(AccentColour2);
            public SolidColorBrush AccentColour2HoverSCBrush => new SolidColorBrush(AccentColour2Hover);
            public SolidColorBrush AccentColour3SCBrush => new SolidColorBrush(AccentColour3);
            public SolidColorBrush AccentColour4SCBrush => new SolidColorBrush(AccentColour4);
            public SolidColorBrush AccentColour5SCBrush => new SolidColorBrush(AccentColour5);
            public SolidColorBrush TextColourSCBrush => new SolidColorBrush(TextColour);
        }

        public class Metadata
        {
            /// <summary> Version of MultiRPC the theme was made for </summary>
            public Version MultiRPCVersion;

            public string ThemeName;
        }
    }
}