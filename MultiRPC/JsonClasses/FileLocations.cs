using System;
using System.IO;

namespace MultiRPC.JsonClasses
{
    public static class FileLocations
    {
        static FileLocations()
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }
        }

        public static string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MultiRPC");
        public static string ConfigFileName = "Config.json";
        public static string ConfigFileLocalLocation = Path.Combine(ConfigFolder, ConfigFileName);

        public static string CreditsFileName = "Credits.json";
        public static string CreditsFileLocalLocation = Path.Combine(ConfigFolder, CreditsFileName);

        public static string ProfilesFileName = "Profiles.json";
        public static string ProfilesFileLocalLocation = Path.Combine(ConfigFolder, ProfilesFileName);

        public static string ChangelogFileName = "Changelog.txt";
        public static string ChangelogFileLocalLocation = Path.Combine(ConfigFolder, ChangelogFileName);

        public static string OpenFileName = "Open.rpc";
        public static string OpenFileLocalLocation = Path.Combine(ConfigFolder, OpenFileName);
        public static string MultiRPCStartLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Discord Universe", "MultiRPC.appref-ms");

    }
}
