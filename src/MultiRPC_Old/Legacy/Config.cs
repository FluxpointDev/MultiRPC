using System.Globalization;
using System.Text.Json;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Legacy;

public class Config
{
    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Config));

    /// <summary> Debug test </summary>
    public bool Debug { get; set; }

    /// <summary> What language is to be shown </summary>
    public string ActiveLanguage { get; set; } = CultureInfo.CurrentUICulture.Name;

    /// <summary> What theme to use </summary>
    public string ActiveTheme { get; set; } = Path.Combine("Assets", "Themes", "DarkTheme" + Constants.LegacyThemeFileExtension);

    /// <summary> Enable showing afk time </summary>
    public bool AFKTime { get; set; }

    /// <summary> Enable starting rich presence when app loads </summary>
    public string AutoStart { get; set; } = Language.GetText(LanguageText.No);

    /// <summary> If to auto update the app </summary>
    public bool AutoUpdate { get; set; }

    /// <summary> If to check the token </summary>
    public bool CheckToken { get; set; } = true;

    /// <summary> What client to connect to </summary>
    public int ClientToUse { get; set; }

    /// <summary> Disabled settings config  </summary>
    public DisableConfig Disabled { get; set; } = new DisableConfig();

    /// <summary> Check if discord is running </summary>
    public bool DiscordCheck { get; set; } = true;

    /// <summary> If to hide the taskbar when minimized </summary>
    public bool HideTaskbarIconWhenMin { get; set; } = true;

    /// <summary> Has the user been warned for invites in rich presence text </summary>
    public bool InviteWarn { get; set; }

    /// <summary> What the user name and 4 digit number was last time this app was ran </summary>
    public string LastUser { get; set; }

    /// <summary> Default rich presence config </summary>
    public DefaultConfig MultiRPC { get; set; } = new DefaultConfig();

    /// <summary> Tells the app what custom button to press in code </summary>
    public int SelectedCustom { get; set; }

    public bool ShowPageTooltips { get; set; } = true;
    
    //This ended up not being used as it was for trigger's which never got into the public release of MultiRPC
    //public bool HadTriggerWarning { get; set; }

    /// <summary> Get the settings stored on disk </summary>
    public static Config? Load()
    {
        if (File.Exists(FileLocations.ConfigFileLocalLocation))
        {
            try
            {
                using var file = File.OpenRead(FileLocations.ConfigFileLocalLocation);
                return JsonSerializer.Deserialize(file, ConfigContext.Default.Config);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        return null;
    }
}

/// <summary> Default rich presence config </summary>
public class DefaultConfig
{
    public int LargeKey { get; set; } = 2;
    public string LargeText { get; set; }
    public bool ShowTime { get; set; }
    public int SmallKey { get; set; }
    public string SmallText { get; set; }
    public string Text1 { get; set; } = Language.GetText(LanguageText.Hello);
    public string Text2 { get; set; } = Language.GetText(LanguageText.World);
    public string? Button1Name { get; set; }
    public string? Button1Url { get; set; }
    public string? Button2Name { get; set; }
    public string? Button2Url { get; set; }
}

/// <summary> Disabled settings config </summary>
public class DisableConfig
{
    /// <summary> Disable the custom tab help icons </summary>
    public bool HelpIcons { get; set; }
}