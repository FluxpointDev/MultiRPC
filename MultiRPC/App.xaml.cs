using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MultiRPC.Functions;
using MultiRPC.GUI.Pages;
using MultiRPC.JsonClasses;
using Newtonsoft.Json;

namespace MultiRPC
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string MultiRPCWebsiteRoot = "https://multirpc.blazedev.me";
        public const int RetryCount = 10; //How many times to attempt downloading files
        public const string AppDev = "Builder#0001";
        public const string ServerInviteCode = "susQ6XA";
        public static UIText Text;
        public static Logging Logging;
        public static bool StartedWithJumpListLogic;
        public static Config Config;
        public static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };


        public App()
        {
            var darkThemeLocation = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);
            var lightThemeLocation = Path.Combine("Assets", "Themes", "LightTheme" + Theme.ThemeExtension);
            var russiaThemeLocation = Path.Combine("Assets", "Themes", "RussiaTheme" + Theme.ThemeExtension);
            if (!File.Exists(darkThemeLocation)) Theme.Save(Theme.Dark, darkThemeLocation);
            if (!File.Exists(lightThemeLocation)) Theme.Save(Theme.Light, lightThemeLocation);
            if (!File.Exists(russiaThemeLocation)) Theme.Save(Theme.Russia, russiaThemeLocation);
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
                        File.Delete(FileLocations.OpenFileLocalLocation);
                    }
                    catch
                    {
                        App.Logging.Application(App.Text.CouldntDelete + " " + FileLocations.OpenFileLocalLocation);
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
                StartedWithJumpListLogic = true;
                _ = CustomPage.JumpListLogic(e.Args[1], true);
            }
#endif

            FileWatch.Create();
            SettingsPage.UIText = new List<UIText>();
            GetLangFiles().ConfigureAwait(false).GetAwaiter().GetResult();
            UITextUpdate().ConfigureAwait(false).GetAwaiter().GetResult();
            Config = Config.Load().Result;
            UITextUpdate().ConfigureAwait(false).GetAwaiter().GetResult();
            Logging = new Logging();
            File.AppendAllText(FileLocations.ErrorFileLocalLocation,
                $"\r\n------------------------------------------------------------------------------------------------\r\n{Text.ErrorsFrom} {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private Task UITextUpdate()
        {
            for (var i = 0; i < SettingsPage.UIText.Count; i++)
            {
                var text = SettingsPage.UIText[i];
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
                    if (text.LanguageTag == "en-gb")
                    {
                        Text = text;
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private Task GetLangFiles()
        {
            var langFiles = Directory.EnumerateFiles("Lang").ToArray();
            for (var i = 0; i < langFiles.LongLength; i++)
                using (var reader = File.OpenText(langFiles[i]))
                {
                    SettingsPage.UIText.Add((UIText) JsonSerializer.Deserialize(reader, typeof(UIText)));
                }

            return Task.CompletedTask;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            File.AppendAllText(FileLocations.ErrorFileLocalLocation,
                $"\r\n\r\n{Text.Message}\r\n{e.Exception.Message}\r\n\r\n{Text.Source}\r\n{e.Exception.Source}\r\n\r\nStackTrace\r\n{e.Exception.StackTrace}");
            Logging.Error(Text.UnhandledException, e.Exception);
        }
    }
}