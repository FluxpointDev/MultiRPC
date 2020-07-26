using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
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
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string MultiRPCWebsiteRoot = "https://multirpc.fluxpoint.dev";
        public const int RetryCount = 10; //How many times to attempt downloading files
        public const string AppDev = "Builderb#0001";
        public const string ServerInviteCode = "TjF6QDC";
        public static UIText Text;
        public static Logging Logging;
        public static bool StartedWithJumpListLogic;
        public static Config Config;

        public static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static bool IsAdministrator =>
           new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public App()
        {
            ProcessWatcher.Start();
            
            var darkThemeLocation = Path.Combine("Assets", "Themes", "DarkTheme" + Theme.ThemeExtension);
            var lightThemeLocation = Path.Combine("Assets", "Themes", "LightTheme" + Theme.ThemeExtension);
            var russiaThemeLocation = Path.Combine("Assets", "Themes", "RussiaTheme" + Theme.ThemeExtension);
            if (!File.Exists(darkThemeLocation))
            {
                Theme.Save(Theme.Dark, darkThemeLocation);
            }

            if (!File.Exists(lightThemeLocation))
            {
                Theme.Save(Theme.Light, lightThemeLocation);
            }

            if (!File.Exists(russiaThemeLocation))
            {
                Theme.Save(Theme.Russia, russiaThemeLocation);
            }

            //TriggerWatch.Start();
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            #if !DEBUG
            var args = e.Args?.ToList() ?? System.Extra.Uri.GetQueryStringParameters() ?? new List<string>();
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
                        Logging.Application(Text.CouldntDelete + " " + FileLocations.OpenFileLocalLocation);
                    }
                }

                if (args.Contains("-custom") && args.IndexOf("-custom") + 1 >= args.Count)
                {
                    File.WriteAllLines(FileLocations.OpenFileLocalLocation,
                        new List<string> { "LOADCUSTOM", args.ElementAt(args.IndexOf("-custom") + 1) });
                }
                else
                {
                    File.Create(FileLocations.OpenFileLocalLocation);
                }

                if (!args.Contains("--fromupdate"))
                {
                    Current.Shutdown();
                }
            }
            else if (args.Contains("-custom") && args.IndexOf("-custom") + 1 >= args.Count)
            {
                StartedWithJumpListLogic = true;
                _ = CustomPage.StartCustomProfileLogic(args.ElementAt(args.IndexOf("-custom") + 1), true);
            }
            #endif

            FileWatch.Create();
            SettingsPage.UIText = new List<UIText>();
            GetLangFiles();
            Config = Config.Load();
            if (Config.Debug)
            {
                try
                {
                    UITextUpdate();
                    Logging = new Logging();
                    File.AppendAllText(FileLocations.ErrorFileLocalLocation,
                        $"\r\n------------------------------------------------------------------------------------------------\r\n{Text.ErrorsFrom} {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
                    DispatcherUnhandledException += App_DispatcherUnhandledException;
                    Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                    Dispatcher.UnhandledException += Dispatcher_UnhandledException;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                UITextUpdate();
                Logging = new Logging();
                File.AppendAllText(FileLocations.ErrorFileLocalLocation,
                    $"\r\n------------------------------------------------------------------------------------------------\r\n{Text.ErrorsFrom} {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
                DispatcherUnhandledException += App_DispatcherUnhandledException;
            }
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Config.Debug)
                MessageBox.Show(e.Exception.ToString(), $"{sender.ToString()} error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (Config.Debug)
                MessageBox.Show(e.Exception.ToString(), $"{sender.ToString()} error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Config.Debug)
                MessageBox.Show(e.ExceptionObject.ToString(), $"{sender.ToString()} error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Config.Debug)
                MessageBox.Show(e.Exception.ToString(), $"{sender.ToString()} error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UITextUpdate()
        {
            var engbInt = 0;
            var foundText = false;
            for (var i = 0; i < SettingsPage.UIText.Count; i++)
            {
                var text = SettingsPage.UIText[i];
                if (Config != null && text != null && text.LanguageTag == Config.ActiveLanguage)
                {
                    Text = text;
                    foundText = true;
                    if (string.IsNullOrWhiteSpace(Config.AutoStart))
                    {
                        Config.AutoStart = Text.No;
                    }

                    if (string.IsNullOrWhiteSpace(Config.MultiRPC.Text1))
                    {
                        Config.MultiRPC.Text1 = Text.Hello;
                    }

                    if (string.IsNullOrWhiteSpace(Config.MultiRPC.Text2))
                    {
                        Config.MultiRPC.Text2 = Text.World;
                    }

                    break;
                }

                if (text != null && text.LanguageTag == "en-GB")
                {
                    engbInt = i;
                }
            }

            if (!foundText)
            {
                Text = SettingsPage.UIText[engbInt];
            }
        }

        private void GetLangFiles()
        {
            var langFiles = Directory.EnumerateFiles("Lang").ToArray();
            for (var i = 0; i < langFiles.LongLength; i++)
            {
                using var reader = File.OpenText(langFiles[i]);
                SettingsPage.UIText.Add((UIText)JsonSerializer.Deserialize(reader, typeof(UIText)));
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Config.Debug)
                MessageBox.Show(e.ToString(), $"{sender.ToString()} error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.AppendAllText(FileLocations.ErrorFileLocalLocation,
                $"\r\n\r\n{Text.Message}\r\n{e.Exception.Message}\r\n\r\n{Text.Source}\r\n{e.Exception.Source}\r\n\r\nStackTrace\r\n{e.Exception.StackTrace}");
            Logging.Error(Text.UnhandledException, e.Exception);
        }
    }
}