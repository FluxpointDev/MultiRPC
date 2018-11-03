using System.IO;
using Newtonsoft.Json;

namespace MultiRPC
{
    public class _Settings
    {
        public _Settings()
        {
            Load();
        }
        public WinType WinType = WinType.Auto;
        public bool ForceCustom = false;
        public string CustomClient;

        public void Save()
        {
            using (StreamWriter file = File.CreateText("Settings.json"))
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, this);
            }
        }
        public void Load()
        {
            if (File.Exists("Settings.json"))
            {
                using (StreamReader reader = new StreamReader("Settings.json"))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    _Settings _Settings = (_Settings)serializer.Deserialize(reader, typeof(_Settings));
                    CustomClient = _Settings.CustomClient;
                    ForceCustom = _Settings.ForceCustom;
                }
            }
        }
    }
}
