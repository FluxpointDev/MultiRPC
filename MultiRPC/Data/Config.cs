using MultiRPC.Data;
using MultiRPC.GUI;
using Newtonsoft.Json;
using System.IO;

namespace MultiRPC
{
    public class Config
    {
        public string LastUser = "";
        /// <summary> Has the user been warned for invites in rich presence text </summary>
        public bool InviteWarn = false;

        /// <summary> Enable showing afk time </summary>
        public bool AFKTime = false;

        /// <summary> Enable starting rich presence when app loads </summary>
        public string AutoStart = "No";

        /// <summary> Default rich presence config </summary>
        public DefaultConfig MultiRPC = new DefaultConfig();

        /// <summary> Disabled settings config  </summary>
        public DisableConfig Disabled = new DisableConfig();

        /// <summary> Only allow one instance of the app </summary>
        public bool Once = true;

        /// <summary> Save the config </summary>
        public void Save()
        {
            using (StreamWriter file = File.CreateText(App.ConfigFile))
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
        public string Text1 = "Hello";
        public string Text2 = "World";
        public int LargeKey = 2;
        public string LargeText;
        public int SmallKey;
        public string SmallText;
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
