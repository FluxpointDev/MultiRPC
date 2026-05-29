using System.Text.Json;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.Theming;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Legacy;

/// <summary>
/// Migrates all data from V5+ into the current version
/// </summary>
public static class MigrateData
{
    public enum MigrateStatus
    {
        Success,
        Failed,
        PartialSuccess,
        NoMigrationNeeded
    }

    private static readonly ILogging Logging = LoggingCreator.CreateLogger(nameof(MigrateData));
    public static (MigrateStatus status, string? failedReason) Migrate()
    {
        if (!Directory.Exists(FileLocations.ConfigFolder))
        {
            return (MigrateStatus.NoMigrationNeeded, null);
        }

        // Get old config
        var oldConfig = Config.Load();

        // Get old profiles
        var oldProfilesExist = File.Exists(FileLocations.ProfilesFileLocalLocation);
        var oldProfilesStream = oldProfilesExist ? File.OpenRead(FileLocations.ProfilesFileLocalLocation) : Stream.Null;
        Dictionary<string, CustomProfile>? oldProfiles = null;
        try
        {
            oldProfiles = oldProfilesExist
                ? JsonSerializer.Deserialize(oldProfilesStream, OldProfilesContext.Default.OldProfiles)
                : null;
            oldProfilesStream.Dispose();
        }
        catch (Exception e)
        {
            oldProfilesStream.Dispose();
            Logging.Error(e);
        }

        //Fail if we wasn't able to successful get anything
        var ableToGetConfig = oldConfig != null || !File.Exists(FileLocations.ConfigFileName);
        var ableToGetOldProfiles = oldProfiles != null || !oldProfilesExist;
        if (!ableToGetConfig && !ableToGetOldProfiles)
        {
            return (MigrateStatus.Failed, Language.GetText(LanguageText.FailedToGetOldData));
        }

        // Process old config
        var profileSettings = SettingManager<ProfilesSettings>.Setting;
        if (oldConfig != null)
        {
            // Process normal settings
            var settings = SettingManager<GeneralSettings>.Setting;
            settings.LastUser = oldConfig.LastUser;
            settings.LogLevel = oldConfig.Debug ? LogLevel.Trace : LogLevel.Info;
            settings.ShowAfkTime = oldConfig.AFKTime;
            settings.ThemeFile = Path.Combine(Constants.ThemeFolder, Path.GetFileName(oldConfig.ActiveTheme));
            settings.Language = oldConfig.ActiveLanguage;
            settings.AutoStart = oldConfig.AutoStart;
            settings.Client = (DiscordClients)oldConfig.ClientToUse;

            // Process disabled settings
            var disabledSettings = SettingManager<DisableSettings>.Setting;
            disabledSettings.AutoUpdate = !oldConfig.AutoUpdate;
            disabledSettings.TokenCheck = !oldConfig.CheckToken;
            disabledSettings.DiscordCheck = !oldConfig.DiscordCheck;
            disabledSettings.HideTaskbarIcon = !oldConfig.HideTaskbarIconWhenMin;
            disabledSettings.InviteWarn = oldConfig.InviteWarn;
            disabledSettings.ShowPageTooltips = !oldConfig.ShowPageTooltips;
            disabledSettings.HelpIcons = oldConfig.Disabled.HelpIcons;

            //TODO: Add large key
            // Process MultiRPC profile
            var multiRPC = SettingManager<MultiRPCSettings>.Setting;
            //multiRPC.Presence.Profile.LargeKey = oldConfig.MultiRPC.LargeKey;
            multiRPC.Presence.Profile.LargeText = oldConfig.MultiRPC.LargeText;
            multiRPC.Presence.Profile.ShowTime = oldConfig.MultiRPC.ShowTime;
            //multiRPC.Presence.Profile.SmallKey = oldConfig.MultiRPC.SmallKey;
            multiRPC.Presence.Profile.SmallText = oldConfig.MultiRPC.SmallText;
            multiRPC.Presence.Profile.Details = oldConfig.MultiRPC.Text1;
            multiRPC.Presence.Profile.State = oldConfig.MultiRPC.Text2;
            multiRPC.Presence.Profile.Button1Text = oldConfig.MultiRPC.Button1Name ?? string.Empty;
            multiRPC.Presence.Profile.Button1Url = oldConfig.MultiRPC.Button1Url ?? string.Empty;
            multiRPC.Presence.Profile.Button2Text = oldConfig.MultiRPC.Button2Name ?? string.Empty;
            multiRPC.Presence.Profile.Button2Url = oldConfig.MultiRPC.Button2Url ?? string.Empty;
        }

        // Process old profiles
        var counter = 0;
        var oldProfileCounter = oldConfig?.SelectedCustom ?? -1;
        if (oldProfiles != null)
        {
            foreach (var (_, profile) in oldProfiles)
            {
                var presence = profile.ToRichPresence();
                if (presence != null && profileSettings.Profiles.All(x => !x.Equals(presence)))
                {
                    profileSettings.Profiles.Add(presence);
                }

                if (counter == oldProfileCounter && presence != null)
                {
                    profileSettings.LastSelectedProfileIndex = profileSettings.Profiles.IndexOf(presence);
                }
                counter++;
            }
        }

        // Copy old theme's
        if (Directory.Exists(FileLocations.ThemesFolder))
        {
            foreach (var file in Directory.EnumerateFiles(FileLocations.ThemesFolder))
            {
                var newFileLoc = Path.Combine(Constants.ThemeFolder, Path.GetFileName(file));
                if (!File.Exists(newFileLoc) && Theme.Load(file) != null)
                {
                    if (!Directory.Exists(Constants.ThemeFolder))
                    {
                        Directory.CreateDirectory(Constants.ThemeFolder);
                    }
                    File.Copy(file, newFileLoc);
                }
            }
        }

        if (!ableToGetConfig)
        {
            return (MigrateStatus.PartialSuccess, Language.GetText(LanguageText.FailedToGetOldConfig));
        }

        if (!ableToGetOldProfiles)
        {
            return (MigrateStatus.PartialSuccess, Language.GetText(LanguageText.FailedToGetOldProfiles));
        }
        return (MigrateStatus.Success, null);
    }
}