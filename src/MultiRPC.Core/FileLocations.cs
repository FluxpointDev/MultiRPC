using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace MultiRPC.Core
{
    /// <summary>
    /// Where all the common files will be
    /// </summary>
    public static class FileLocations
    {
        /// <summary>
        /// Where all the settings file should be contained
        /// </summary>
        public static string ConfigFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MultiRPC");

        /// <summary>
        /// Where all the users themes should be contained
        /// </summary>
        public static string ThemesFolder => Path.Combine(ConfigFolder, "Themes");

        /// <summary>
        /// Old Settings file name
        /// </summary>
        public static string ConfigFileName => "Config.json";

        /// <summary>
        /// Old Settings file location
        /// </summary>
        public static string ConfigFileLocation => Path.Combine(ConfigFolder, ConfigFileName);

        /// <summary>
        /// Credits file name
        /// </summary>
        public static string CreditsFileName => "Credits.json";

        /// <summary>
        /// Credits file location
        /// </summary>
        public static string CreditsFileLocation => Path.Combine(ConfigFolder, CreditsFileName);

        /// <summary>
        /// Custom Profiles file name
        /// </summary>
        public static string ProfilesFileName => "Profiles.json";

        /// <summary>
        /// Custom Profiles file location
        /// </summary>
        public static string ProfilesFileLocation => Path.Combine(ConfigFolder, ProfilesFileName);

        /// <summary>
        /// Changelog file name
        /// </summary>
        public static string ChangelogFileName => "Changelog.txt";

        /// <summary>
        /// Changelog file location
        /// </summary>
        public static string ChangelogFileLocation => Path.Combine(ConfigFolder, ChangelogFileName);

        /// <summary>
        /// Errors file name
        /// </summary>
        public static string ErrorFileName => "Error.txt";

        /// <summary>
        /// Errors file location
        /// </summary>
        public static string ErrorFileLocation => Path.Combine(ConfigFolder, ErrorFileName);

        /// <summary>
        /// file to check for opening application up
        /// </summary>
        public static string OpenFileName => "Open.rpc";

        /// <summary>
        /// Open file location
        /// </summary>
        public static string OpenFileLocation => Path.Combine(ConfigFolder, OpenFileName);

        //TODO: To change based on OS and platform
        /// <summary>
        /// Application shortcut file location
        /// </summary>
        public static string MultiRPCStartLink =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "MultiRPC.appref-ms");

        //TODO: See if this is working
        static FileLocations()
        {
            var fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();

            if (!fileSystem.DirectoryExists(ConfigFolder))
            {
                fileSystem.CreateDirectory(ConfigFolder);
            }

            if (!fileSystem.DirectoryExists(ThemesFolder))
            {
                fileSystem.CreateDirectory(ThemesFolder);
            }
        }
    }
}