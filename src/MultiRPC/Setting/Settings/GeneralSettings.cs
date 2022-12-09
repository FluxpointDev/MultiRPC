using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting.Settings.Attributes;
using MultiRPC.UI.Pages;
using PropertyChanged.SourceGenerator;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Setting.Settings;

public partial class GeneralSettings : IBaseSetting<GeneralSettings>
{
    public static string Name => "General";

    public static JsonTypeInfo<GeneralSettings> TypeInfo { get; } = GeneralSettingsContext.Default.GeneralSettings;

    [Notify]
    private string? _lastUser = "NA#0000";

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"Client\")]")]
    [Notify]
    private DiscordClients _client;

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"AutoStart\")]")]
    [PropertyAttribute($"[MultiRPC.Setting.Settings.Attributes.SettingSource(\"{nameof(GetAutoStarts)}\")]")]
    [Notify]
    private string _autoStart = "";

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"Language\")]")]
    [PropertyAttribute($"[MultiRPC.Setting.Settings.Attributes.SettingSource(\"{nameof(GetLanguages)}\")]")]
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.NoneLocalizable]")]
    [Notify]
    private string _language = "";
        
    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"LogLevel\")]")]
    [Notify]
    private LogLevel _logLevel = LogLevel.Trace;

    [PropertyAttribute("[MultiRPC.Setting.Settings.Attributes.SettingName(\"AfkTime\")]")]
    [Notify]
    private bool _showAfkTime;

    [Notify]
    private string? _themeFile;
    
    private void OnLogLevelChanged(LogLevel previous, LogLevel value) => LoggingCreator.GlobalLevel = value;
    private void OnLanguageChanged(string previous, string value)
    {
        if (Languages.ContainsKey(value))
        {
            LanguageGrab.ChangeLanguage(Languages[value]);
        }
    }

    //TODO: Move this into languagegrab
    internal static readonly Dictionary<string, string> Languages = new Dictionary<string, string>();
    internal static string[] GetLanguages()
    {
        if (Languages.Any())
        {
            return Languages.Keys.ToArray();
        }
            
        foreach (var file in Directory.GetFiles(Constants.LanguageFolder, "*.json"))
        {
            var lines = File.ReadLines(file);
            var line = lines.First(x => x.Contains("LanguageName"));
            var langName = line[(line.IndexOf(':') + 1)..].Trim()[1..^2];
            Languages.Add(langName, file);
        }

        return Languages.Keys.ToArray();
    }

    private string[] GetAutoStarts()
    {
        var l = new List<string> { "No" };
        l.AddRange(PageManager.CurrentPages
            .Where(x => x is IRpcPage)
            .Select(x => x.LocalizableName));
        return l.ToArray();
    }
}