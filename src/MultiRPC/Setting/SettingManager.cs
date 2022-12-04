using System.Text.Json;
using System.ComponentModel;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Setting;

public static class SettingManager<T> 
    where T : BaseSetting, new()
{
    private static readonly Lazy<T> LazySetting = new Lazy<T>(() =>
    {
        T setting = new T();

        var settingFileLocation = Path.Combine(Constants.SettingsFolder, setting.Name + ".json");
        if (File.Exists(settingFileLocation))
        {
            using var fileSteam = File.OpenRead(settingFileLocation);
            try
            {
                var fileSetting = JsonSerializer.Deserialize<T>(fileSteam);
                if (fileSetting != null)
                {
                    setting = fileSetting;
                }
            }
            catch (Exception e)
            {
                LoggingCreator.CreateLogger(nameof(SettingManager<T>)).Error(e);
            }
        }

        if (setting is INotifyPropertyChanged settingNotify)
        {
            settingNotify.PropertyChanged += (sender, args) =>
            {
                if (!Directory.Exists(Constants.SettingsFolder))
                {
                    Directory.CreateDirectory(Constants.SettingsFolder);
                }
                setting.Save();
            };
        }
        return setting;
    });

    public static T Setting => LazySetting.Value;
}