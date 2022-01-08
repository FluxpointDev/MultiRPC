using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace MultiRPC.Setting;

//TODO: See if we can make use of static for SerializerContext?
public static class SettingManager<T> 
    where T : BaseSetting, new()
{
    [UnconditionalSuppressMessage("Trimming", 
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "We allow SerializerContext to be used when possible")]
    private static readonly Lazy<T> LazySetting = new Lazy<T>(() =>
    {
        var setting = new T();
            
        var settingFileLocation = Path.Combine(Constants.SettingsFolder, setting.Name + ".json");
        if (File.Exists(settingFileLocation))
        {
            using var fileSteam = File.OpenRead(settingFileLocation);
            var fileSetting = setting.SerializerContext != null ?
                JsonSerializer.Deserialize<T>(fileSteam, setting.SerializerContext.Options) 
                : JsonSerializer.Deserialize<T>(fileSteam);

            if (fileSetting != null)
            {
                setting = fileSetting;
            }
        }

        if (setting is INotifyPropertyChanged settingNotify)
        {
            settingNotify.PropertyChanged += (sender, args) =>
            {
                setting.Save();
            };
        }
        return setting;
    });

    public static T Setting => LazySetting.Value;
}