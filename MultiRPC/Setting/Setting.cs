using System.IO;
using System.Text.Json;

namespace MultiRPC.Setting
{
    public abstract class Setting
    {
        public abstract string Name { get; }

        public void Save()
        {
            var settingFileLocation = Path.Combine(Constants.SettingsFolder, Name + ".json");
            File.WriteAllText(settingFileLocation, JsonSerializer.Serialize(this, GetType(), Constants.JsonSerializer));
        }
    }
}