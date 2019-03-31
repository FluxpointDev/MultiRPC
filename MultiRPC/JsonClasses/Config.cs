using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using MultiRPC.JsonClasses;

namespace MultiRPC
{
    public class Config
    {
        /// <summary> Tells the app what custom button to press in code </summary>
        public int SelectedCustom = 0;

        /// <summary> If to hide the taskbar when minimized </summary>
        public bool HideTaskbarIconWhenMin = true;

        /// <summary> What language is to be shown </summary>
        public string ActiveLanguage = "English";

        /// <summary> What theme to use </summary>
        public Theme.ActiveTheme ActiveTheme = Theme.ActiveTheme.Dark;

        /// <summary> If to auto update the app </summary>
        public bool AutoUpdate = false;

        /// <summary> What the user name and 4 digit number was last time this app was ran </summary>
        public string LastUser = "";

        /// <summary> Has the user been warned for invites in rich presence text </summary>
        public bool InviteWarn = false;

        /// <summary> Enable showing afk time </summary>
        public bool AFKTime = false;

        /// <summary> Enable starting rich presence when app loads </summary>
        public string AutoStart = App.Text.No;

        /// <summary> If to check the token </summary>
        public bool CheckToken = true;

        /// <summary> Check if discord is running </summary>
        public bool DiscordCheck = true;

        /// <summary> Default rich presence config </summary>
        public DefaultConfig MultiRPC = new DefaultConfig();

        /// <summary> Disabled settings config  </summary>
        public DisableConfig Disabled = new DisableConfig();

        /// <summary> What client to connect to </summary>
        public int ClientToUse = 0;

        /// <summary> Get the settings stored on disk </summary>
        public static async Task<Config> Load()
        {
            if (File.Exists(FileLocations.ConfigFileLocalLocation))
            {
                using (var file = File.OpenText(FileLocations.ConfigFileLocalLocation))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    };
                    return (Config) serializer.Deserialize(file, typeof(Config));
                }
            }
            return new Config();
        }

        /// <summary> Save the config </summary>
        public async Task Save()
        {
            using (StreamWriter file = File.CreateText(FileLocations.ConfigFileLocalLocation))
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, this);
            }
        }
    }

    /// <summary> Default rich presence config </summary>
    public class DefaultConfig
    {
        public string Text1 = App.Text.Hello;
        public string Text2 = App.Text.World;
        public int LargeKey = 2;
        public string LargeText;
        public int SmallKey;
        public string SmallText;
        public bool ShowTime = false;
    }

    /// <summary> Disabled settings config </summary>
    public class DisableConfig
    {
        /// <summary> Disable the programs tab </summary>
        public bool ProgramsTab = false;

        /// <summary> Disable the custom tab help icons </summary>
        public bool HelpIcons = false;
    }
}