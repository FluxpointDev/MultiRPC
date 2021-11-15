using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
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
        private static Dictionary<string, string> _languageJsonFileContent = null!;
        private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Language));

        private static void GrabContent()
        {
            //TODO: Be able to change language
            var currentLang = Thread.CurrentThread.CurrentUICulture.Name.ToLower();
            var currentLangTwoLetter = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
            _englishLanguageJsonFileContent = GrabLanguage("en-gb")!;
            if (currentLang != "en-gb")
            {
                _languageJsonFileContent = 
                    GrabLanguage(currentLang) 
                    ?? GrabLanguage(currentLangTwoLetter) 
                    ?? _englishLanguageJsonFileContent;
            }
            _languageJsonFileContent ??= _englishLanguageJsonFileContent;
        }
        
        private static Dictionary<string, string>? GrabLanguage(string name)
        {
            var fileLocation = Path.Combine(Constants.LanguageFolder, name + ".json");
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
            return _languageJsonFileContent.ContainsKey(jsonName) || _englishLanguageJsonFileContent.ContainsKey(jsonName);
        }

        public static string GetText(string? jsonName)
        {
            if (string.IsNullOrWhiteSpace(jsonName))
            {
                return "N/A";
            }
            if (_languageJsonFileContent.ContainsKey(jsonName))
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
            _observables.Add(observer);
            observer.OnNext(Text);
            return new DisposableAction(() => _observables.Remove(observer));
        }

        private IEnumerable<string> GetText() => _jsonNames.Select(Language.GetText);
    }

    public class DisposableAction : IDisposable
    {
        private readonly Action _action;
        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action.Invoke();
            GC.SuppressFinalize(this);
        }
    }
}