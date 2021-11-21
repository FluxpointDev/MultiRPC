using System.IO;
using System.Text.Json;

namespace MultiRPC.Setting
{
    /// <summary>
    /// Base class which contains settings which are to be stored for later use
    /// </summary>
    /// <remarks>
    /// Name will need [JsonIgnore] or it will be stored with your settings.
    /// Where you put your settings in your actual setting class matters on where it shows up in the UI! 
    /// </remarks>
    public abstract class Setting
    {
        public abstract string Name { get; }

        public void Save()
        {
            var settingFileLocation = Path.Combine(Constants.SettingsFolder, Name + ".json");
            if (File.Exists(settingFileLocation))
            {
                File.Delete(settingFileLocation);
            }
            var stream = File.OpenWrite(settingFileLocation);
            JsonSerializer.Serialize(stream, this, GetType(), Constants.JsonSerializer);
            stream.Dispose();
        }
    }
}