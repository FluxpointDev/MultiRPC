using System.ComponentModel;
using System.IO;
using System.Text.Json;
using SharpCompress;

namespace MultiRPC.Setting
{
    public static class SettingManager<T> 
        where T : Setting, new()
    {
        private static readonly Lazy<T> _setting = new Lazy<T>(() =>
        {
            var setting = new T();
            
            var settingFileLocation = Path.Combine(Constants.SettingsFolder, setting.Name + ".json");
            if (File.Exists(settingFileLocation))
            {
                using var fileSteam = File.OpenRead(settingFileLocation);
                var fileSetting = JsonSerializer.Deserialize<T>(fileSteam);
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

        public static T Setting => _setting.Value;
    }
}