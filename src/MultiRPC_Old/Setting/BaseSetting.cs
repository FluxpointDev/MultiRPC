using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MultiRPC.Setting;

public interface IBaseSetting
{
    static abstract string Name { get; }
}

/// <summary>
/// Base class which contains settings which are to be stored for later use
/// </summary>
/// <remarks>
/// Where you put your settings in your actual setting class matters on where it shows up in the UI!
/// </remarks>
public interface IBaseSetting<TSelf> : IBaseSetting
    where TSelf : IBaseSetting<TSelf>
{
    static abstract JsonTypeInfo<TSelf> TypeInfo { get; }

    void Save()
    {
        var settingFileLocation = Path.Combine(Constants.SettingsFolder, TSelf.Name + ".json");
        if (File.Exists(settingFileLocation))
        {
            File.Delete(settingFileLocation);
        }
        var stream = File.OpenWrite(settingFileLocation);

        JsonSerializer.Serialize(stream, (TSelf)this, TSelf.TypeInfo);
        stream.Dispose();
    }
}