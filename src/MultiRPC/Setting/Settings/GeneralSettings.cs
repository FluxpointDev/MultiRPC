using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Fonderie;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting.Settings.Attributes;
using MultiRPC.UI.Pages;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Setting.Settings;

public partial class GeneralSettings : BaseSetting
{
    [JsonIgnore]
    public override string Name => "General";

    [GeneratedProperty]
    private string? _lastUser = "NA#0000";

    [GeneratedProperty, SettingName("Client")]
    private DiscordClients _client;

    [GeneratedProperty, SettingName("AutoStart"), SettingSource(nameof(GetAutoStarts))]
    private string _autoStart = "";

    [GeneratedProperty, SettingName("Language"), SettingSource(nameof(GetLanguages)), NoneLocalizable]
    private string _language = "";
        
    [GeneratedProperty, SettingName("LogLevel")]
    private LogLevel _logLevel = LogLevel.Trace;

    [GeneratedProperty, SettingName("AfkTime")]
    private bool _showAfkTime;

    [GeneratedProperty]
    private string? _themeFile;
    
    partial void OnLogLevelChanged(LogLevel previous, LogLevel value) => LoggingCreator.GlobalLevel = value;
    partial void OnLanguageChanged(string previous, string value)
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
            .Where(x => x is RpcPage)
            .Select(x => x.LocalizableName));
        return l.ToArray();
    }
}