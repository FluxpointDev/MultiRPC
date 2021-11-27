using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC
{
    public class Language
    {
        private readonly Lazy<LanguageObservable> _textObservable;
        private static Dictionary<string, string> _englishLanguageJsonFileContent = null!;
        private static Dictionary<string, string>? _languageJsonFileContent;
        private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Language));
        static Language()
        {
            GrabLanguage();
        }

        public Language() : this(LanguageText.NA) { }
        public Language(params LanguageText[] jsonNames) : this(jsonNames.Select(x => x.ToString()).ToArray()) { }
        public Language(params string[] jsonNames)
        {
            _textObservable = new Lazy<LanguageObservable>(() => new LanguageObservable(jsonNames));
        }

        public void ChangeJsonNames(params LanguageText[] jsonNames) => ChangeJsonNames(jsonNames.Select(x => x.ToString()).ToArray());
        public void ChangeJsonNames(params string[] jsonNames)
        {
            _textObservable.Value.ChangeJsonNames(jsonNames);
        }

        public LanguageObservable TextObservable => _textObservable.Value;
        public string Text => TextObservable.Text;
        public static event EventHandler? LanguageChanged;

        internal static void ChangeLanguage(string file)
        {
            var lan = GrabLanguageFile(file);
            if (lan != null)
            {
                _languageJsonFileContent = lan;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        
        private static void GrabLanguage()
        {
            _englishLanguageJsonFileContent = GrabLanguageFile(GetFilePath("en-gb"))!;

            //If we have a set language then we want to load the current languages we have
            var langName = SettingManager<GeneralSettings>.Setting.Language;
            if (!string.IsNullOrWhiteSpace(langName))
            {
                //Only load it in if we have the language
                GeneralSettings.GetLanguages();
                if (GeneralSettings.Languages.ContainsKey(langName))
                {
                    _languageJsonFileContent = GrabLanguageFile(GeneralSettings.Languages[langName]);
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
                    GrabLanguageFile(GetFilePath(currentLang)) 
                    ?? GrabLanguageFile(GetFilePath(currentLangTwoLetter));
            }
        }

        private static string GetFilePath(string name) => Path.Combine(Constants.LanguageFolder, name + ".json");
        private static Dictionary<string, string>? GrabLanguageFile(string fileLocation)
        {
            if (!File.Exists(fileLocation))
            {
                return null;
            }
           
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(fileLocation), 
                    new JsonSerializerOptions(JsonSerializerDefaults.General)
                    {
                        //We know that the file in question is safe as we provide the files 
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
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

        public static string GetText(LanguageText langText) => GetText(langText.ToString());
        public static string GetText(string? jsonName)
        {
            if (string.IsNullOrWhiteSpace(jsonName))
            {
                return GetText(LanguageText.NA);
            }
            if (_languageJsonFileContent?.ContainsKey(jsonName) ?? false)
            {
                return _languageJsonFileContent[jsonName];
            }
            //We manually have the N/A here in case it isn't seen anywhere (But that would also show a bigger issue)
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