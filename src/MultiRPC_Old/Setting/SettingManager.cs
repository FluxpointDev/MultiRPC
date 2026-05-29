using System.Text.Json;
using System.ComponentModel;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Setting;

public static class SettingManager<TSetting> 
    where TSetting : IBaseSetting<TSetting>, new()
{
    private static readonly Lazy<TSetting> LazySetting = new(() =>
    {
        TSetting? setting = default;

        var settingFileLocation = Path.Combine(Constants.SettingsFolder, TSetting.Name + ".json");
        if (File.Exists(settingFileLocation))
        {
            using var fileSteam = File.OpenRead(settingFileLocation);
            try
            {
                var fileSetting = JsonSerializer.Deserialize(fileSteam, TSetting.TypeInfo);
                if (fileSetting != null)
                {
                    setting = fileSetting;
                }
            }
            catch (Exception e)
            {
                LoggingCreator.CreateLogger(nameof(SettingManager<TSetting>)).Error(e);
            }
        }
        
        setting ??= new TSetting();
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

    public static TSetting Setting => LazySetting.Value;
}