namespace MultiRPC.Legacy;

public static class FileLocations
{
    public static readonly string ConfigFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MultiRPC");

    public static readonly string ThemesFolder = Path.Combine(ConfigFolder, "Themes");

    public const string ConfigFileName = "Config.json";
    public static readonly string ConfigFileLocalLocation = Path.Combine(ConfigFolder, ConfigFileName);

    private const string ProfilesFileName = "Profiles.json";
    public static readonly string ProfilesFileLocalLocation = Path.Combine(ConfigFolder, ProfilesFileName);

    private const string ChangelogFileName = "Changelog.txt";
    public static readonly string ChangelogFileLocalLocation = Path.Combine(ConfigFolder, ChangelogFileName);
}