using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiRPC.Setting
{
    /// <summary>
    /// Base class which contains settings which are to be stored for later use
    /// </summary>
    /// <remarks>
    /// Name will need [JsonIgnore] or it will be stored with your settings.
    /// Where you put your settings in your actual setting class matters on where it shows up in the UI! 
    /// </remarks>
    public abstract class BaseSetting
    {
        public abstract string Name { get; }

        public abstract JsonSerializerContext? SerializerContext { get; }

        [UnconditionalSuppressMessage("Trimming", 
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
            Justification = "We allow SerializerContext to be used when possible")]
        public void Save()
        {
            var settingFileLocation = Path.Combine(Constants.SettingsFolder, Name + ".json");
            if (File.Exists(settingFileLocation))
            {
                File.Delete(settingFileLocation);
            }
            var stream = File.OpenWrite(settingFileLocation);

            if (SerializerContext != null)
            {
                JsonSerializer.Serialize(stream, this, GetType(), SerializerContext);
            }
            else
            {
                JsonSerializer.Serialize(stream, this, GetType(), Constants.JsonSerializer);
            }
            stream.Dispose();
        }
    }
}