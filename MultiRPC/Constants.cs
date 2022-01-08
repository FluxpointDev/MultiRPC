using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace MultiRPC;

/// <summary>
/// Contains objects that will not change value at any time throughout the span of the clients usage
/// </summary>
public static class Constants
{
    static Constants()
    {
        // Windows apps have restricted access, use the appdata folder instead of document.
        SettingsFolder =
#if _UWP
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages/29025FluxpointDevelopment.MultiRPC_q026kjacpk46y/AppData");
#else
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MultiRPC-Beta");
#endif
        ThemeFolder = Path.Combine(SettingsFolder, "Themes");
    }

    /// <summary>
    /// MultiRPC Application ID
    /// </summary>
    public const long MultiRPCID = 450894077165043722;

    /// <summary>
    /// Afk Application ID
    /// </summary>
    public const long AfkID = 469643793851744257;

    /// <summary>
    /// How many times you should attempt downloading files
    /// </summary>
    public const int RetryCount = 10;

    /// <summary>
    /// Url to the MultiRPCs info + download page
    /// </summary>
    public const string WebsiteUrl = "https://fluxpoint.dev/multirpc";

    /// <summary>
    /// The app developer
    /// </summary>
    public const string AppDeveloper = "Fluxpoint Development";

    /// <summary>
    /// The discord server's invite code
    /// </summary>
    public const string ServerInviteCode = "TjF6QDC";

    /// <summary>
    /// Url for the discord server
    /// </summary>
    public const string DiscordServerUrl = "https://discord.gg/" + ServerInviteCode;

    /// <summary>
    /// Serializer for JSON
    /// </summary>
    public static JsonSerializerOptions JsonSerializer { get; } = new JsonSerializerOptions
    {
        WriteIndented = true
    };
        
    /// <summary>
    /// Where all Settings should be stored
    /// </summary>
    public static string SettingsFolder { get; }

    /// <summary>
    /// The theme's file extension
    /// </summary>
    public const string LegacyThemeFileExtension = ".multirpctheme";

    /// <summary>
    /// The theme's file extension
    /// </summary>
    public const string ThemeFileExtension = ".multitheme";

    public static string ThemeFolder { get; }

    public static Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version!;

    /// <summary>
    /// The folder with all the languages
    /// </summary>
    public static string LanguageFolder { get; } = Path.Combine("Assets", "Language");
}