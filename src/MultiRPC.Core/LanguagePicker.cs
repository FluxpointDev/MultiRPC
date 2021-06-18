using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MultiRPC.Core.Extensions;
using Serilog;

namespace MultiRPC.Core
{
    //TODO: Cleanup code
    /// <summary>
    /// Helps with grabbing language contents
    /// </summary>
    public static class LanguagePicker
    {
        static LanguagePicker()
        {
            GrabContents();
        }
        
        static void GrabContents()
        {
            //TODO: Add something to know if this is already happening and to just wait for that one to finish
            if (EnglishLanguageJsonFileContent != null)
            {
                return;
            }

            //TODO: Get active language
            var fileLocation = Path.Combine(Constants.LanguageFolder, "en-gb.json");
            if (File.Exists(fileLocation))
            {
                Log.Logger.Debug("File exists, grabbing contents");
                var fileContents = File.ReadAllText(fileLocation);
                Log.Logger.Debug("Grabbed contents");
                if (string.IsNullOrWhiteSpace(fileContents))
                {
                    Log.Logger.Warning("Unable to get language contents");
                    return;
                }

                try
                {
                    Log.Logger.Debug("Parsing");
                    EnglishLanguageJsonFileContent = JsonSerializer.Deserialize<Dictionary<string, string>>(fileContents);
                    LanguageJsonFileContent = EnglishLanguageJsonFileContent;
                    Log.Logger.Debug("Parsed!");
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e);
                }
            }
        }

        private static Dictionary<string, string> EnglishLanguageJsonFileContent;
        private static Dictionary<string, string> LanguageJsonFileContent;

        /// <summary>
        /// Gets the line that the language file contains
        /// </summary>
        /// <param name="jsonName">Name of the line you want to grab</param>
        public static string GetLineFromLanguageFile(string? jsonName)
        {
            if (string.IsNullOrWhiteSpace(jsonName))
            {
                return "N/A";
            }
            if (LanguageJsonFileContent.ContainsKey(jsonName))
            {
                return LanguageJsonFileContent[jsonName];
            }
            if (EnglishLanguageJsonFileContent.ContainsKey(jsonName))
            {
                return EnglishLanguageJsonFileContent[jsonName];
            }

            return "N/A";
        }
    }
}