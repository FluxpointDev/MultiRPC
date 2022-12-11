using SemVersion;
using System.Reflection;
using System.Text.Json;
using TinyUpdate.Core.Extensions;

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
        LogFolder = Path.Combine(SettingsFolder, "Logging");
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
    /// Url to the github repo
    /// </summary>
    public const string GithubUrl = "https://github.com/FluxpointDev/MultiRPC";

    /// <summary>
    /// Url to donate to fluxpoint
    /// </summary>
    public const string DonateUrl = "https://fluxpoint.dev/donate";

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
    /// Where all the logging should go if being put onto disk
    /// </summary>
    public static string LogFolder { get; }

    /// <summary>
    /// The theme's file extension
    /// </summary>
    public const string LegacyThemeFileExtension = ".multirpctheme";

    /// <summary>
    /// The theme's file extension
    /// </summary>
    public const string ThemeFileExtension = ".multitheme";

    /// <summary>
    /// Where all the theme's are stored
    /// </summary>
    public static string ThemeFolder { get; }

    /// <summary>
    /// What is the current version of MultiRPC!
    /// </summary>
    public static readonly SemanticVersion CurrentVersion = Assembly.GetExecutingAssembly().GetSemanticVersion()!;

    /// <summary>
    /// The folder with all the languages
    /// </summary>
    public static string LanguageFolder { get; } = Path.Combine("Assets", "Language");
}