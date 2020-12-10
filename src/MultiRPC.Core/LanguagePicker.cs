#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MultiRPC.Core
{
    /// <summary>
    /// Helps with grabbing language contents
    /// </summary>
    public static class LanguagePicker
    {
        static async Task GrabContents()
        {
            //TODO: Add something to know if this is already happering and to just wait for that one to finish
            if (EnglishLanguageJsonFileContent != null)
            {
                return;
            }

            IFileSystemAccess fileSystem;
            try
            {
                fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return;
            }

            //TODO: Get active language
            var fileLocation = Path.Combine(Constants.LanguageFolder, "en-gb.json");
            //if (await fileSystem.FileExists(fileLocation))
            {
                Log.Logger.Debug("File exists, grabbing contents");
                var fileContents = await fileSystem.ReadAllTextAsync(fileLocation);
                Log.Logger.Debug("Grabbed contents");
                if (string.IsNullOrWhiteSpace(fileContents))
                {
                    Log.Logger.Warning("Unable to get language contents");
                    return;
                }

                try
                {
                    Log.Logger.Debug($"Parsing");
                    EnglishLanguageJsonFileContent = JObject.Parse(fileContents);
                    LanguageJsonFileContent = EnglishLanguageJsonFileContent;
                    Log.Logger.Debug($"Parsed!");
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.Message);
                }
            }
        }

        internal static JObject EnglishLanguageJsonFileContent;
        internal static JObject LanguageJsonFileContent;

        /// <summary>
        /// Gets the line that the language file contains
        /// </summary>
        /// <param name="jsonName">Name of the line you want to grab</param>
        public static async Task<string?> GetLineFromLanguageFile(string jsonName)
        {
            Log.Logger.Debug("Making sure that we got contents");
            await GrabContents();
            Log.Logger.Debug($"Have contents: {LanguageJsonFileContent != null}");

            return !string.IsNullOrWhiteSpace(jsonName) ?
            (LanguageJsonFileContent?.Value<string>(jsonName) ?? EnglishLanguageJsonFileContent?.Value<string>(jsonName)) :
            "N/A";
        }
    }
}