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
        public CustomConfig Custom;
        public bool InviteWarn = false;
        public bool AFKTime = false;
        public void Save(MainWindow window = null)
        {
            if (window != null)
            {
                Custom = new CustomConfig
                {
                    ID = ulong.Parse(window.Text_CustomClientID.Text),
                    Text1 = window.Text_CustomText1.Text,
                    Text2 = window.Text_CustomText2.Text,
                    LargeKey = window.Text_CustomLargeKey.Text,
                    LargeText = window.Text_CustomLargeText.Text,
                    SmallKey = window.Text_CustomSmallKey.Text,
                    SmallText = window.Text_CustomSmallText.Text
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
