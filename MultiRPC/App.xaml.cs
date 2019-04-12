using System;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;
using MultiRPC.Functions;
using MultiRPC.GUI.Pages;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.Generic;

namespace MultiRPC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static UIText Text;
        public static Logging Logging;
        public static WebClient WebClient = new WebClient();
        public const string MuiltiRPCWebsiteRoot = "https://multirpc.blazedev.me";
        public static JsonSerializer JsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        public static Config Config;
        public const int RetryCount = 10; //How many times to attempt downloading files
        public const string AppDev = "Builder#0001";
        public const string ServerInviteCode = "susQ6XA";

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            if (Process.GetProcessesByName("MultiRPC").Length > 1)
            {
                if (File.Exists(FileLocations.OpenFileLocalLocation))
                {
                    try
                    {
                        File.Delete(Path.Combine(FileLocations.OpenFileLocalLocation));
                    }
                    catch
                    {

                    }
                }
                if (e.Args.Length > 0)
                {
                    File.WriteAllLines(FileLocations.OpenFileLocalLocation, new List<string> { "LOADCUSTOM", e.Args[1] });
                }
                else
                {
                    File.Create(FileLocations.OpenFileLocalLocation);
                }
                Current.Shutdown();
            }
            else if (e.Args.Length > 1 && e.Args[0] == "-custom")
            {
                CustomPage.JumpListLogic(e.Args[1], true);
            }
#endif

            FileWatch.Create();
            SettingsPage.UIText = new List<UIText>();
            GetLangFiles().ConfigureAwait(false).GetAwaiter().GetResult();
            UITextUpdate().ConfigureAwait(false).GetAwaiter().GetResult();
            Config = Config.Load().Result;
            UITextUpdate().ConfigureAwait(false).GetAwaiter().GetResult();
            Logging = new Logging();
            File.AppendAllText(FileLocations.ErrorFileLocalLocation, $"\r\n------------------------------------------------------------------------------------------------\r\n{App.Text.ErrorsFrom} {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private async Task UITextUpdate()
        {
            foreach (var text in SettingsPage.UIText)
            {
                if (Config != null)
                {
                    if (text.LanguageName == Config.ActiveLanguage)
                    {
                        Text = text;
                        break;
                    }
                }
                else
                {
                    if (text.LanguageName == "English")
                    {
                        Text = text;
                        break;
                    }
                }
            }
        }

        public async Task GetLangFiles()
        {
            foreach (var LangFile in Directory.EnumerateFiles("Lang"))
            {
                using (var reader = File.OpenText(LangFile))
                {
                    SettingsPage.UIText.Add((UIText) JsonSerializer.Deserialize(reader, typeof(UIText)));
                }
            }
            
            using (var reader = File.OpenText("Lang/english.json"))
                Text = (UIText)JsonSerializer.Deserialize(reader, typeof(UIText));
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            File.AppendAllText(FileLocations.ErrorFileLocalLocation, $"\r\n\r\n{App.Text.Message}\r\n{e.Exception.Message}\r\n\r\n{App.Text.Source}\r\n{e.Exception.Source}\r\n\r\nStackTrace\r\n{e.Exception.StackTrace}");
            Logging.Error(Text.UnhandledException, e.Exception);
        }
    }
}
