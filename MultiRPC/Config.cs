using MultiRPC.GUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC
{
    public class Config
    {
        public DefaultConfig MultiRPC;
        public CustomConfig Custom;
        public bool InviteWarn = false;
        public bool AFKTime = false;
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
    public class DefaultConfig
    {
        public string Text1;
        public string Text2;
        public int LargeKey;
        public string LargeText;
        public int SmallKey;
        public string SmallText;
    }
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
}
