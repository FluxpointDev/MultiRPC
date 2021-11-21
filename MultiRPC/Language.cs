using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Threading;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC
{
    public class Language
    {
        static Language()
        {
            GrabContent();
        }

        public Language(params string[] jsonNames)
        {
            _textObservable = new Lazy<LanguageObservable>(new LanguageObservable(jsonNames));
        }

        public void ChangeJsonNames(params string[] jsonNames)
        {
            _textObservable.Value.ChangeJsonNames(jsonNames);
        }
        private readonly Lazy<LanguageObservable> _textObservable;
        public LanguageObservable TextObservable => _textObservable.Value;
        public string Text => TextObservable.Text;

        private static Dictionary<string, string> _englishLanguageJsonFileContent = null!;
        private static Dictionary<string, string>? _languageJsonFileContent;
        private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Language));

        public static event EventHandler? LanguageChanged;
        internal static void ChangeLanguage(string file)
        {
            var lan = GrabLanguage(file);
            if (lan != null)
            {
                _languageJsonFileContent = lan;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        
        private static void GrabContent()
        {
            _englishLanguageJsonFileContent = GrabLanguage(GetFilePath("en-gb"))!;

            //If we have a set language then we want to load the current languages we have
            var langName = SettingManager<GeneralSettings>.Setting.Language;
            if (!string.IsNullOrWhiteSpace(langName))
            {
                //Only load it in if we have the language
                GeneralSettings.GetLanguages();
                if (GeneralSettings.Languages.ContainsKey(langName))
                {
                    _languageJsonFileContent = GrabLanguage(GeneralSettings.Languages[langName]);
                }
            }

            if (_languageJsonFileContent != null)
                return;

            /*If we wasn't able to load the users language from
             settings in, try to load in from what the system is*/
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            var currentLang = currentCulture.Name.ToLower();
            
            //Don't try if the system is en-gb as we already load that in as a fallback
            if (currentLang != "en-gb")
            {
                var currentLangTwoLetter = currentCulture.TwoLetterISOLanguageName.ToLower();
                _languageJsonFileContent =
                    GrabLanguage(GetFilePath(currentLang)) 
                    ?? GrabLanguage(GetFilePath(currentLangTwoLetter));
            }
        }

        private static string GetFilePath(string name) =>
            Path.Combine(Constants.LanguageFolder, name + ".json");

        private static Dictionary<string, string>? GrabLanguage(string fileLocation)
        {
            if (!File.Exists(fileLocation))
            {
                return null;
            }
           
            Logger.Debug("{0} exists, grabbing contents", fileLocation);
            using var fileContentsStream = File.OpenRead(fileLocation);
            
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(fileContentsStream)!;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return null;
        }

        public static bool HasKey(string jsonName)
        {
            return (_languageJsonFileContent?.ContainsKey(jsonName) ?? false) || _englishLanguageJsonFileContent.ContainsKey(jsonName);
        }

        public static string GetText(string? jsonName)
        {
            if (string.IsNullOrWhiteSpace(jsonName))
            {
                return "N/A";
            }
            if (_languageJsonFileContent?.ContainsKey(jsonName) ?? false)
            {
                return _languageJsonFileContent[jsonName];
            }
            return _englishLanguageJsonFileContent.ContainsKey(jsonName) ? _englishLanguageJsonFileContent[jsonName] : "N/A";
        }
    }

    public class LanguageObservable : IObservable<string>
    {
        public LanguageObservable(params string[] jsonNames)
        {
            _jsonNames = jsonNames;
        }

        internal void ChangeJsonNames(params string[] jsonNames)
        {
            _jsonNames = jsonNames;
            _observables.ForEach(x => x.OnNext(Text));
        }

        internal string Text => string.Join(' ', GetText());
        
        private string[] _jsonNames;
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<IObserver<string>> _observables = new List<IObserver<string>>();
        public IDisposable Subscribe(IObserver<string> observer)
        {
            Language.LanguageChanged += LanguageOnLanguageChanged;
            _observables.Add(observer);
            observer.OnNext(Text);
            return Disposable.Create(() => _observables.Remove(observer));
        }

        private void LanguageOnLanguageChanged(object? sender, EventArgs e)
        {
            _observables.ForEach(x => x.OnNext(Text));
        }

        private IEnumerable<string> GetText() => _jsonNames.Select(Language.GetText);
    }
}