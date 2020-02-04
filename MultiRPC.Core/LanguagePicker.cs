#nullable enable

using System.IO;
using Newtonsoft.Json.Linq;

namespace MultiRPC.Core
{
    /// <summary>
    /// Helps with grabbing language contents
    /// </summary>
    public static class LanguagePicker
    {
        internal static JObject EnglishLanguageJsonFileContent = JObject.Parse(File.ReadAllText(Path.Combine(Constants.LanguageFolder, (Settings.Current?.ActiveLanguage ?? "en-gb") + ".json")));
        internal static JObject LanguageJsonFileContent = EnglishLanguageJsonFileContent;

        /// <summary>
        /// Gets the line that the language file contains
        /// </summary>
        /// <param name="jsonName">Name of the line you want to grab</param>
        public static string? GetLineFromLanguageFile(string jsonName) => !string.IsNullOrWhiteSpace(jsonName) ?
            (LanguageJsonFileContent?.Value<string>(jsonName) ?? EnglishLanguageJsonFileContent?.Value<string>(jsonName)) :
            null;
    }
}