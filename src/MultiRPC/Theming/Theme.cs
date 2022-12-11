using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using MultiRPC.Converters;
using MultiRPC.Extensions;
using MultiRPC.UI;
using PropertyChanged.SourceGenerator;
using SemVersion;
using TinyUpdate.Core.Logging;
using TinyUpdate.Core.Utils;

namespace MultiRPC.Theming;

public partial class Theme
{
    internal string _filepath;
    internal bool _hasAssets = false;
    private ZipArchive? _archive;
    private bool _archiveLoaded = false;

    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Theme));
    private static readonly SemanticVersion ModernVersion = new SemanticVersion(7, 0, null);
    private static readonly JsonTypeInfo<Colours> ColoursJsonContext;
    private static readonly JsonTypeInfo<Metadata> MetadataJsonContext;
    static Theme()
    {
        ColoursJsonContext = new ColoursContext(new JsonSerializerOptions(JsonSerializerDefaults.General)
        { Converters = { new ColourJsonConverter() }}).Colours;
        
        MetadataJsonContext = new MetadataContext(new JsonSerializerOptions(JsonSerializerDefaults.General)
        { Converters = { new VersionJsonConverter() }}).Metadata;
    }
    
    [Notify]
    private bool isBeingEdited;

    /// <summary>
    /// The theme that is actively being used by the application
    /// </summary>
    public static Theme? ActiveTheme { get; private set; }

    /// <summary>
    /// What colouring is in the theme
    /// </summary>
    public Colours Colours { get; set; } = null!;
    
    /// <summary>
    /// Any metadata about this theme
    /// </summary>
    public Metadata Metadata { get; set; } = null!;

    /// <summary>
    /// Where the theme is stored
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// What mode this theme is in
    /// </summary>
    public ThemeType ThemeType { get; internal set; } = ThemeType.Legacy;
    
    public static event EventHandler<Theme>? NewTheme;
    
    public static event EventHandler<Theme>? ActiveThemeChanged;

    public bool HaveAsset(string key)
    {
        if (ThemeType == ThemeType.Modern && _hasAssets)
        {
            if (!_archiveLoaded && !ReloadAssets())
            {
                return false;
            }
            return GetEntries(key)?.Any() ?? false;
        }

        //We can't load in assets from the older theming due to it using WPF
        return false;
    }

    public Stream GetAssetStream(string key)
    {
        if (ThemeType != ThemeType.Modern)
        {
            return Stream.Null;
        }
        if (!_archiveLoaded && !ReloadAssets())
        {
            return Stream.Null;
        }

        return GetEntries(key)?.FirstOrDefault()?.Open() ?? Stream.Null;
    }

    internal IEnumerable<ZipArchiveEntry>? GetEntries(string key) => _archive?.Entries.Where(x => x.FullName.StartsWith("Assets/" + key));

    //TODO: Add Edit queue system so we only update what we actually edit
    private static ZipArchiveEntry GetEntry(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name);
        entry?.Delete();
        return archive.CreateEntry(name);
    }

    public bool Save(string? filename)
    {
        //We need to unload assets as we can't write if we don't
        UnloadAssets();

        var fireNewTheme = false;
        if (string.IsNullOrWhiteSpace(Location))
        {
            fireNewTheme = true;
            Directory.CreateDirectory(Constants.ThemeFolder);
            Location = Path.Combine(Constants.ThemeFolder, filename + Constants.ThemeFileExtension);
        }

        Stream? fileStream = null;
        if (File.Exists(Location))
        {
            fileStream = File.Open(Location, FileMode.Open, FileAccess.ReadWrite);
        }
        fileStream ??= File.Create(Location);
        var archive = new ZipArchive(fileStream, ZipArchiveMode.Update);

        var coloursEntry = GetEntry(archive, "colours.json");
        var colourStream = coloursEntry.Open();
        JsonSerializer.Serialize(colourStream, this.Colours, ColoursJsonContext);
        colourStream.Dispose();

        var metadataEntry = GetEntry(archive, "metadataEntry.json");
        var metadataStream = metadataEntry.Open();
        Metadata.Version = Constants.CurrentVersion;
        JsonSerializer.Serialize(metadataStream, this.Metadata, MetadataJsonContext);
        metadataStream.Dispose();

        archive.Dispose();
        if (fireNewTheme)
        {
            NewTheme?.Invoke(null, this);
        }
        return true;
    }

    /// <summary>
    /// Load's in the theme for being used
    /// </summary>
    /// <param name="filepath">Where the file is</param>
    /// <returns>The theme if we successfully loaded it in</returns>
    public static Theme? Load(string? filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath) || filepath[0] == '#')
        {
            return null;
        }
        
        if (!File.Exists(filepath))
        {
            Logger.Error($"{{0}} {Language.GetText(LanguageText.DoesntExist)}!!!", filepath);
            return null;
        }

        var fileStream = StreamUtil.SafeOpenRead(filepath);
        if (fileStream == null)
        {
            Logger.Error(Language.GetText(LanguageText.CantOpenOrSaveThemeFile)
                .Replace("{open/save}", "{0}"), Language.GetText(LanguageText.Open));
            return null;
        }

        ZipArchive archive;
        try
        {
            archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.CantOpenOrSaveThemeFile)
                .Replace("{open/save}", "{0}"), Language.GetText(LanguageText.Open));
            return null;
        }
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
            _archiveLoaded = true,
            _hasAssets = archive.Entries.Any(x => x.FullName.StartsWith("Assets/"))
        };
        try
        {
            theme.Colours = JsonSerializer.Deserialize(coloursEntry.Open(), ColoursJsonContext)!;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.SomethingHappenedWhileGettingThemeColours)
                .Replace("{themepath}", "{0}"), filepath);
            archive.Dispose();
            return null;
        }

        try
        {
            theme.Metadata = JsonSerializer.Deserialize(metadataEntry.Open(), MetadataJsonContext)!;

            if (theme.Metadata.Version >= ModernVersion)
            {
                theme.ThemeType = ThemeType.Modern;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            Logger.Error(Language.GetText(LanguageText.SomethingHappenedWhileGettingThemeColours)
                .Replace("{themepath}", "{0}"), filepath);
            archive.Dispose();
            return null;
        }

        //If this is a legacy theme or it contains no assets then we don't need the archive anymore
        if (theme.ThemeType == ThemeType.Legacy || !theme._hasAssets)
        {
            archive.Dispose();
            theme._archiveLoaded = false;
        }
        return theme;
    }

    public bool ReloadAssets()
    {
        UnloadAssets();
        
        var fileStream = StreamUtil.SafeOpenRead(_filepath);
        if (fileStream != null)
        {
            _archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            _archiveLoaded = true;
            return true;
        }

        return false;
    }
    
    public void UnloadAssets()
    {
        _archive?.Dispose();
        _archiveLoaded = false;
    }

    /// <summary>
    /// Applies the theming to a <see cref="IResourceDictionary"/> (or <see cref="App.Current.Resources"/> if nothing is passed)
    /// </summary>
    /// <param name="resourceDictionary">IResourceDictionary to apply to</param>
    /// <param name="fireAssetChange">If we should fire asset changes</param>
    public void Apply(IResourceDictionary? resourceDictionary = null, bool fireAssetChange = true)
    {
        bool newActiveTheme = resourceDictionary == null;
        if (newActiveTheme)
        {
            resourceDictionary = Application.Current.Resources;
            ActiveTheme?.UnloadAssets();
            ActiveTheme = this;
        }
        resourceDictionary.UpdateIfDifferent("ThemeAccentColor", Colours.ThemeAccentColor);
        resourceDictionary.UpdateIfDifferent("ThemeAccentColor2", Colours.ThemeAccentColor2);
        resourceDictionary.UpdateIfDifferent("ThemeAccentColor2Hover", Colours.ThemeAccentColor2Hover);
        resourceDictionary.UpdateIfDifferent("ThemeAccentColor3", Colours.ThemeAccentColor3);
        resourceDictionary.UpdateIfDifferent("ThemeAccentColor4", Colours.ThemeAccentColor4);
        resourceDictionary.UpdateIfDifferent("ThemeAccentColor5", Colours.ThemeAccentColor5);

        resourceDictionary.UpdateIfDifferent("ThemeAccentDisabledColor", Colours.ThemeAccentDisabledColor);
        resourceDictionary.UpdateIfDifferent("ThemeAccentDisabledTextColor", Colours.ThemeAccentDisabledTextColor);
        resourceDictionary.UpdateIfDifferent("NavButtonSelectedColor", Colours.NavButtonSelectedColor);
        resourceDictionary.UpdateIfDifferent("NavButtonSelectedIconColor", Colours.NavButtonSelectedIconColor);
        resourceDictionary.UpdateIfDifferent("ThemeForegroundBrush",  new ImmutableSolidColorBrush(Colours.TextColour));

        resourceDictionary.UpdateIfDifferent("CheckBoxForegroundUnchecked", resourceDictionary["ThemeForegroundBrush"]);
        resourceDictionary.UpdateIfDifferent("CheckBoxForegroundChecked", resourceDictionary["ThemeForegroundBrush"]);
        resourceDictionary.UpdateIfDifferent("CheckBoxForegroundCheckedPointerOver", resourceDictionary["ThemeForegroundBrush"]);
        resourceDictionary.UpdateIfDifferent("CheckBoxForegroundUncheckedPointerOver", resourceDictionary["ThemeForegroundBrush"]);
        resourceDictionary.UpdateIfDifferent("CheckBoxForegroundCheckedPressed", resourceDictionary["ThemeForegroundBrush"]);
        resourceDictionary.UpdateIfDifferent("CheckBoxForegroundUncheckedPressed", resourceDictionary["ThemeForegroundBrush"]);

        var color4Brush = new ImmutableSolidColorBrush((Color)resourceDictionary["ThemeAccentColor4"]!);
        var color5Brush = new ImmutableSolidColorBrush((Color)resourceDictionary["ThemeAccentColor5"]!);
        resourceDictionary.UpdateIfDifferent("CheckBoxCheckBackgroundStrokeUnchecked", color4Brush);
        resourceDictionary.UpdateIfDifferent("CheckBoxCheckBackgroundStrokeChecked", color4Brush);
        resourceDictionary.UpdateIfDifferent("CheckBoxCheckBackgroundStrokeCheckedPointerOver", color5Brush);
        resourceDictionary.UpdateIfDifferent("CheckBoxCheckBackgroundStrokeUncheckedPointerOver", color5Brush);
        resourceDictionary.UpdateIfDifferent("CheckBoxCheckBackgroundStrokeCheckedPressed", color5Brush);
        resourceDictionary.UpdateIfDifferent("CheckBoxCheckBackgroundStrokeUncheckedPressed", color5Brush);
        
        resourceDictionary.UpdateIfDifferent("ToolTipForeground", resourceDictionary["ThemeForegroundBrush"]);
        resourceDictionary.UpdateIfDifferent("ToolTipBackground", resourceDictionary["ThemeAccentBrush"]);
        resourceDictionary.UpdateIfDifferent("ToolTipBorderBrush", resourceDictionary["ThemeAccentBrush4"]);
        if (fireAssetChange)
        {
            AssetManager.FireReloadAssets(this);
        }

        if (newActiveTheme)
        {
            ActiveThemeChanged?.Invoke(this, ActiveTheme);
        }
    }
    
    public Theme Clone(string? name = null)
    {
        var blacklistedNames = 
#if !THEME_EDIT
            Themes.ThemeIndexes.Keys.Select(x => x[1..]);
#else
            ArraySegment<string>.Empty;
#endif
        return new Theme()
        {
            Metadata = new Metadata(FileExt.CheckFilename(name ?? this.Metadata.Name, Constants.ThemeFolder, blacklistedNames), this.Metadata.Version),
            Colours = JsonSerializer.Deserialize(JsonSerializer.Serialize(this.Colours, ColoursJsonContext), ColoursJsonContext)!,
            ThemeType = this.ThemeType,
            Location = null,
        };
    }
}