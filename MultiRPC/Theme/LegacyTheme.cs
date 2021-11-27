using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using MultiRPC.Theme.JsonConverter;
using TinyUpdate.Core.Logging;
using TinyUpdate.Core.Utils;

namespace MultiRPC.Theme;
public class LegacyTheme
{
    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(LegacyTheme));

    public LegacyColours Colours { get; private set; }
    public LegacyMetadata Metadata { get; private set; }

    public static LegacyTheme? Load(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Logger.Error($"{filepath} {Language.GetText(LanguageText.DoesntExist)}!!!");
            return null;
        }

        var fileStream = StreamUtil.SafeOpenRead(filepath);
        if (fileStream == null)
        {
            Logger.Error(Language.GetText(LanguageText.CantOpenOrSaveThemeFile)
                .Replace("{open/save}", Language.GetText(LanguageText.Open)));
            return null;
        }

        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var coloursEntry = archive.GetEntry("colours.json");
        var metadataEntry = archive.GetEntry("metadataEntry.json");
        if (coloursEntry == null || metadataEntry == null)
        {
            return null;
        }

        var theme = new LegacyTheme();
        try
        {
            theme.Colours = JsonSerializer.Deserialize<LegacyColours>(coloursEntry.Open(),
                new JsonSerializerOptions(JsonSerializerDefaults.General)
                {
                    Converters = { new LegacyColourJsonConverter() }
                });
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.SomethingHappenedWhileGettingThemeColours)
                .Replace("{themepath}", filepath));
            return null;
        }

        try
        {
            theme.Metadata = JsonSerializer.Deserialize<LegacyMetadata>(metadataEntry.Open(),
                new JsonSerializerOptions(JsonSerializerDefaults.General)
                {
                    Converters = { new LegacyVersionJsonConverter() }
                });
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.SomethingHappenedWhileGettingThemeMetadata)
                .Replace("{themepath}", filepath));
            return null;
        }

        return theme;
    }

    public void Apply(IResourceDictionary? resourceDictionary = null)
    {
        resourceDictionary ??= Application.Current.Resources;
        resourceDictionary["ThemeAccentColor"] = Colours.ThemeAccentColor;
        resourceDictionary["ThemeAccentColor2"] = Colours.ThemeAccentColor2;
        resourceDictionary["ThemeAccentColor2Hover"] = Colours.ThemeAccentColor2Hover;
        resourceDictionary["ThemeAccentColor3"] = Colours.ThemeAccentColor3;
        resourceDictionary["ThemeAccentColor4"] = Colours.ThemeAccentColor4;
        resourceDictionary["ThemeAccentColor5"] = Colours.ThemeAccentColor5;

        resourceDictionary["ThemeAccentDisabledColor"] = Colours.ThemeAccentDisabledColor;
        resourceDictionary["ThemeAccentDisabledTextColor"] = Colours.ThemeAccentDisabledTextColor;
        resourceDictionary["NavButtonSelectedColor"] = Colours.NavButtonSelectedColor;
        resourceDictionary["NavButtonSelectedIconColor"] = Colours.NavButtonSelectedIconColor;
        resourceDictionary["TextColour"] = Colours.TextColour;
    }
}