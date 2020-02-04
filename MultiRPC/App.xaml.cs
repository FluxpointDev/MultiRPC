using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MultiRPC.Core;
using MultiRPC.Core.Managers;
using MultiRPC.Core.Rpc;
using MultiRPC.GUI;
using MultiRPC.Managers;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Notification;

namespace MultiRPC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Managers<Page, SolidColorBrush> Manager => Managers<Page, SolidColorBrush>.Current;
        private FileWatch RpcFileWatch;

        private App()
        {
            Managers<Page, SolidColorBrush>.Setup(new MainPageManager(), new MultiRPCIcons());

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Exit += App_Exit;
            NetworkChange.NetworkAddressChanged += (sender, e) => NotificationCenter.Logger.Information($"Does the user have internet access?: {Utils.NetworkIsAvailable()}");

            Startup += (_, __) => new MainWindow(new MainPage()).Show();
            RpcFileWatch = new FileWatch(fileExtension: "*.rpc");
            RpcFileWatch.Created += FileWatcher_Created;
        }

        private async void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.Name != FileLocations.OpenFileName)
            {
                return;
            }

            await MainWindow.Dispatcher.InvokeAsync(async () =>
            {
                //Give it time to be written to.
                await Task.Delay(1000);

                string[] text;
                using (var reader = File.OpenText(FileLocations.OpenFileLocation))
                {
                    text = (await reader.ReadToEndAsync()).Split('\r', '\n');
                }

                //if (text[0] == "LOADCUSTOM") //Load a custom profile
                //{
                //    CustomPage.StartCustomProfileLogic(text[2]);
                //}
                //else
                {
                    Current.MainWindow.WindowState = WindowState.Normal;
                    Current.MainWindow.Activate();
                }

                try
                {
                    File.Delete(FileLocations.OpenFileLocation);
                }
                catch
                {
                    NotificationCenter.Logger.Error($"{LanguagePicker.GetLineFromLanguageFile("CouldntDelete")} {FileLocations.OpenFileLocation}");
                }
            });
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Rpc.StopRpc();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.GetType() == typeof(NotImplementedException))
            {
                //Show that hey, this is planned and has UI in place but nothing behind it 💕
                return;
            }

            System.Diagnostics.Debugger.Break();
            NotificationCenter.Logger.Error(e.Exception);
            e.Handled = true;
            //ToDo: Show Errors™
        }
    }
}
