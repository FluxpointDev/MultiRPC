using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MultiRPC.GUI.Pages;
using MultiRPC.JsonClasses;

namespace MultiRPC.Functions
{
    public static class FileWatch
    {
        public static void Create()
        {
            var watcher = new FileSystemWatcher
            {
                Filter = "*.rpc",
                Path = FileLocations.ConfigFolder,
                EnableRaisingEvents = true
            };
            watcher.Created += Watcher_FileCreated;
        }

        private static async void Watcher_FileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.Name == FileLocations.OpenFileName)
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await Application.Current.MainWindow.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(1000);
                        string[] text;
                        using (var reader = File.OpenText(FileLocations.OpenFileLocalLocation))
                        {
                            text = (await reader.ReadToEndAsync()).Split('\r', '\n');
                        }

                        if (text[0] == "LOADCUSTOM") //Load a custom profile
                        {
                            CustomPage.StartCustomProfileLogic(text[2]);
                        }
                        else
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                            Application.Current.MainWindow.Activate();
                        }

                        try
                        {
                            File.Delete(FileLocations.OpenFileLocalLocation);
                        }
                        catch
                        {
                            App.Logging.Application($"{App.Text.CouldntDelete} {FileLocations.OpenFileLocalLocation}");
                        }
                    });
                });
            }
        }
    }
}