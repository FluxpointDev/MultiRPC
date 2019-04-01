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
            FileSystemWatcher watcher = new FileSystemWatcher
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
                await App.Current.Dispatcher.InvokeAsync(async  () => 
                { 
                    await App.Current.MainWindow.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(1000);
                        string[] text;
                        using (StreamReader reader = File.OpenText(FileLocations.OpenFileLocalLocation))
                            text = (await reader.ReadToEndAsync()).Split('\r', '\n');

                        if (text[0] == "LOADCUSTOM") //Load a custom profile
                        {
                            CustomPage.JumpListLogic(text[2]);
                        }
                        else
                        {
                            App.Current.MainWindow.WindowState = WindowState.Normal;
                            App.Current.MainWindow.Activate();
                        }

                        try
                        {
                            File.Delete(Path.Combine(FileLocations.OpenFileLocalLocation));
                        }
                        catch
                        {
                        }
                    });
                });
            }
        }
    }

}
