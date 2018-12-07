using MultiRPC.GUI;
using Newtonsoft.Json;
using System.IO;

namespace MultiRPC
{
    public class Config
    {
        /// <summary> Has the user been warned for invites in rich presence text </summary>
        public bool InviteWarn = false;

        /// <summary> Enable showing afk time </summary>
        public bool AFKTime = false;

        /// <summary> Enable starting rich presence when app loads </summary>
        public string AutoStart = "No";

        /// <summary> Default rich presence config </summary>
        public DefaultConfig MultiRPC;

        /// <summary> Custom rich presence config [OLD] </summary>
        public CustomConfig Custom;

        /// <summary> Disabled settings config  </summary>
        public DisableConfig Disabled = new DisableConfig();

        /// <summary> Only allow one instance of the app </summary>
        public bool Once = true;

        /// <summary> Save the config </summary>
        public void Save(MainWindow window = null)
        {
            if (window != null)
            {
                Custom = new CustomConfig
                {
                    Text1 = window.TextCustomText1.Text,
                    Text2 = window.TextCustomText2.Text,
                    LargeKey = window.TextCustomLargeKey.Text,
                    LargeText = window.TextCustomLargeText.Text,
                    SmallKey = window.TextCustomSmallKey.Text,
                    SmallText = window.TextCustomSmallText.Text
                };
                if (ulong.TryParse(window.TextCustomClientID.Text, out ulong id))
                    Custom.ID = id;
                MultiRPC = new DefaultConfig
                {
                    Text1 = window.TextDefaultText1.Text,
                    Text2 = window.TextDefaultText2.Text,
                    LargeKey = window.ItemsDefaultLarge.SelectedIndex,
                    LargeText = window.TextDefaultLarge.Text,
                    SmallKey = window.ItemsDefaultSmall.SelectedIndex,
                    SmallText = window.TextDefaultSmall.Text
                };
            }
            using (StreamWriter file = File.CreateText(RPC.ConfigFile))
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
        public string Text1;
        public string Text2;
        public int LargeKey;
        public string LargeText;
        public int SmallKey;
        public string SmallText;
    }

    /// <summary> Custom rich presence config </summary>
    public class CustomConfig
    {
        public ulong ID;
        public string Text1;
        public string Text2;
        public string LargeKey;
        public string LargeText;
        public string SmallKey;
        public string SmallText;
    }

    /// <summary> Disabled settings config </summary>
    public class DisableConfig
    {
        // Force check if Discord process is running on RPC start
        public bool DiscordCheck = false;

        /// <summary> Test </summary>
        public bool TokenCheck = false;

        /// <summary> Disable the programs tab </summary>
        public bool ProgramsTab = false;

        /// <summary> Disable the custom tab help icons </summary>
        public bool HelpIcons = false;
    }
}
