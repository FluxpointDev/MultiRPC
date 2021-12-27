using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MultiRPC.Theming.JsonConverter;
using TinyUpdate.Core.Logging;
using TinyUpdate.Core.Utils;

namespace MultiRPC.Theming;

public enum ThemeMode 
{
    /// <summary>
    /// This theme was made before V7
    /// </summary>
    Legacy,
    /// <summary>
    /// This theme was made in V7+
    /// </summary>
    Modern
}

public class Theme : IDisposable
{
    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Theme));
    private static readonly Version ModernVersion = new Version(7, 0);
    private static readonly JsonTypeInfo<Colours> ColoursJsonContext;
    private static readonly JsonTypeInfo<Metadata> MetadataJsonContext;
    static Theme()
    {
        ColoursJsonContext = new ColoursContext(new JsonSerializerOptions(JsonSerializerDefaults.General)
        { Converters = { new ColourJsonConverter() }}).Colours;
        
        MetadataJsonContext = new MetadataContext(new JsonSerializerOptions(JsonSerializerDefaults.General)
        { Converters = { new LegacyVersionJsonConverter() }}).Metadata;
    }
    
    /// <summary>
    /// The theme that is currently being used
    /// </summary>
    public static Theme? ActiveTheme { get; internal set; }

    /// <summary>
    /// What colouring that we need to apply
    /// </summary>
    public Colours Colours { get; set; } = null!;
    
    /// <summary>
    /// Any metadata about this theme
    /// </summary>
    public Metadata Metadata { get; set; } = null!;

    /// <summary>
    /// Where the theme is currently stored
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// What mode this theme is in
    /// </summary>
    public ThemeMode Mode { get; private set; } = ThemeMode.Legacy;

    public bool HaveAsset(string key)
    {
        if (Mode == ThemeMode.Modern)
        {
            if (!_archiveLoaded)
            {
                ReloadAssets();
            }
            return _archive?.Entries.Any(x => x.FullName == "Assets/" + key) ?? false;
        }

        //We can't load in assets from the older theming
        return false;
    }

    public Stream GetAssetStream(string key)
    {
        if (Mode != ThemeMode.Modern)
        {
            return Stream.Null;
        }
        if (!_archiveLoaded)
        {
            ReloadAssets();
        }

        return _archive?.Entries
            .FirstOrDefault(x => x.FullName == "Assets/" + key)?
            .Open() ?? Stream.Null;
    }

    private string _filepath = null!;
    private ZipArchive? _archive;
    /// <summary>
    /// Load's in the theme for being used
    /// </summary>
    /// <param name="filepath">Where the file is</param>
    /// <returns>The theme if we successfully loaded it in</returns>
    public static Theme? Load(string? filepath)
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

        var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var coloursEntry = archive.GetEntry("colours.json");
        var metadataEntry = archive.GetEntry("metadataEntry.json");
        if (coloursEntry == null || metadataEntry == null)
        {
            return null;
        }

        var theme = new Theme
        {
            Location = filepath,
            _archive = archive,
            _filepath = filepath,
            _archiveLoaded = true
        };
        try
        {
            theme.Colours = JsonSerializer.Deserialize(coloursEntry.Open(), ColoursJsonContext)!;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.SomethingHappenedWhileGettingThemeColours)
                .Replace("{themepath}", filepath));
            archive.Dispose();
            return null;
        }

        try
        {
            theme.Metadata = JsonSerializer.Deserialize(metadataEntry.Open(), MetadataJsonContext)!;

            if (theme.Metadata.Version >= ModernVersion)
            {
                theme.Mode = ThemeMode.Modern;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.SomethingHappenedWhileGettingThemeMetadata)
                .Replace("{themepath}", filepath));
            archive.Dispose();
            return null;
        }

        //If this is a legacy theme then we don't need the archive anymore
        if (theme.Mode == ThemeMode.Legacy)
        {
            archive.Dispose();
            theme._archiveLoaded = false;
        }
        return theme;
    }

    private bool _archiveLoaded = false;
    public void ReloadAssets()
    {
        UnloadAssets();
        
        var fileStream = StreamUtil.SafeOpenRead(_filepath);
        if (fileStream != null)
        {
            _archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            _archiveLoaded = true;
        }
    }
    
    public void UnloadAssets()
    {
        _archive?.Dispose();
        _archiveLoaded = false;
    }
    
    /// <summary>
    /// Applies the theming to a IResourceDictionary (or App.Current.Resources if nothing is applied)
    /// </summary>
    /// <param name="resourceDictionary">IResourceDictionary to apply to</param>
    public void Apply(IResourceDictionary? resourceDictionary = null)
    {
        if (resourceDictionary == null)
        {
            resourceDictionary = Application.Current.Resources;
            ActiveTheme?.UnloadAssets();
            ActiveTheme = this;
        }
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
        resourceDictionary["ThemeForegroundBrush"] = new SolidColorBrush(Colours.TextColour);

        resourceDictionary["CheckBoxForegroundUnchecked"] = resourceDictionary["ThemeForegroundBrush"];
        resourceDictionary["CheckBoxForegroundChecked"] = resourceDictionary["ThemeForegroundBrush"];
        resourceDictionary["CheckBoxForegroundCheckedPointerOver"] = resourceDictionary["ThemeForegroundBrush"];
        resourceDictionary["CheckBoxForegroundUncheckedPointerOver"] = resourceDictionary["ThemeForegroundBrush"];
        resourceDictionary["CheckBoxForegroundCheckedPressed"] = resourceDictionary["ThemeForegroundBrush"];
        resourceDictionary["CheckBoxForegroundUncheckedPressed"] = resourceDictionary["ThemeForegroundBrush"];
        
        AssetManager.FireReloadAssets(this);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}