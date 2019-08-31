using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MultiRPC.JsonClasses
{
    public class Config
    {
        /// <summary> What language is to be shown </summary>
        public string ActiveLanguage = "en-gb";

        /// <summary> What theme to use </summary>
        public string ActiveTheme = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);

        /// <summary> Enable showing afk time </summary>
        public bool AFKTime = false;

        /// <summary> Enable starting rich presence when app loads </summary>
        public string AutoStart = App.Text?.No;

        /// <summary> If to auto update the app </summary>
        public bool AutoUpdate = false;

        /// <summary> If to check the token </summary>
        public bool CheckToken = true;

        /// <summary> What client to connect to </summary>
        public int ClientToUse = 0;

        /// <summary> Disabled settings config  </summary>
        public DisableConfig Disabled = new DisableConfig();

        /// <summary> Check if discord is running </summary>
        public bool DiscordCheck = true;

        /// <summary> If to hide the taskbar when minimized </summary>
        public bool HideTaskbarIconWhenMin = true;

        /// <summary> Has the user been warned for invites in rich presence text </summary>
        public bool InviteWarn = false;

        /// <summary> What the user name and 4 digit number was last time this app was ran </summary>
        public string LastUser = "";

        /// <summary> Default rich presence config </summary>
        public DefaultConfig MultiRPC = new DefaultConfig();

        /// <summary> Tells the app what custom button to press in code </summary>
        public int SelectedCustom = 0;

        public bool ShowPageTooltips = true;

        /// <summary> Get the settings stored on disk </summary>
        public static Task<Config> Load()
        {
            if (File.Exists(FileLocations.ConfigFileLocalLocation))
            {
                using (var file = File.OpenText(FileLocations.ConfigFileLocalLocation))
                {
                    var serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    };
                    return Task.FromResult((Config) serializer.Deserialize(file, typeof(Config)));
                }
            }

            return Task.FromResult(new Config());
        }

        /// <summary> Save the config </summary>
        public Task Save()
        {
            using (var file = File.CreateText(FileLocations.ConfigFileLocalLocation))
            {
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, this);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary> Default rich presence config </summary>
    public class DefaultConfig
    {
        public int LargeKey = 2;
        public string LargeText;
        public bool ShowTime = false;
        public int SmallKey;
        public string SmallText;
        public string Text1 = App.Text?.Hello;
        public string Text2 = App.Text?.World;
    }

    /// <summary> Disabled settings config </summary>
    public class DisableConfig
    {
        /// <summary> Disable the custom tab help icons </summary>
        public bool HelpIcons = false;
    }
}