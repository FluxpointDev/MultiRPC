#nullable enable

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace MultiRPC.Core
{
    /// <summary>
    /// Helps with grabbing language contents
    /// </summary>
    public static class LanguagePicker
    {
        static void GrabContents() => Task.Run(async () =>
        {
            if (EnglishLanguageJsonFileContent != null)
            {
                return;
            }

            var fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();

            //TODO: Get active language
            var fileLocation = Path.Combine(Constants.LanguageFolder, "en-gb.json");
            if (await fileSystem.FileExists(fileLocation))
            {
                var fileContents = await fileSystem.ReadAllTextAsync(fileLocation);
                if (string.IsNullOrWhiteSpace(fileContents))
                {
                    Serilog.Log.Logger.Warning("Unable to get lanuage contents");
                    return;
                }

                try
                {
                    EnglishLanguageJsonFileContent = JObject.Parse(fileContents);
                    LanguageJsonFileContent = EnglishLanguageJsonFileContent;
                }
                catch (System.Exception e)
                {
                    Serilog.Log.Logger.Error(e.Message);
                }
            }
        }).Wait();

        internal static JObject EnglishLanguageJsonFileContent;
        internal static JObject LanguageJsonFileContent;

        /// <summary>
        /// Gets the line that the language file contains
        /// </summary>
        /// <param name="jsonName">Name of the line you want to grab</param>
        public static string? GetLineFromLanguageFile(string jsonName)
        {
            GrabContents();

            return !string.IsNullOrWhiteSpace(jsonName) ?
            (LanguageJsonFileContent?.Value<string>(jsonName) ?? EnglishLanguageJsonFileContent?.Value<string>(jsonName)) :
            null;
        }
    }
}