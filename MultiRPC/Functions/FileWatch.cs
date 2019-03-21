using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                App.Current.Dispatcher.Invoke(() => 
                { 
                    App.Current.MainWindow.Dispatcher.Invoke(async () =>
                    {
                        App.Current.MainWindow.WindowState = WindowState.Normal;
                        App.Current.MainWindow.Activate();
                        await Task.Delay(1000);
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
