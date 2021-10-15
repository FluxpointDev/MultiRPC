using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        
        private static Dictionary<string, string> _englishLanguageJsonFileContent;
        private static Dictionary<string, string> _languageJsonFileContent;
        private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Language));
        static void GrabContent()
        {
            //TODO: Get active language
            //TODO: Be able to change language
            var fileLocation = Path.Combine(Constants.LanguageFolder, "en-gb.json");
            if (!File.Exists(fileLocation))
            {
                return;
            }

            Logger.Debug("File exists, grabbing contents from language file");
            using var fileContentsStream = File.OpenRead(fileLocation);

            try
            {
                Logger.Debug("Parsing language file");
                
                _englishLanguageJsonFileContent = JsonSerializer.Deserialize<Dictionary<string, string>>(fileContentsStream)!;
                _languageJsonFileContent = _englishLanguageJsonFileContent;
                Logger.Debug("Parsed file!");
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public string Text => TextObservable.Text;
        
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
        
        private IEnumerable<string> GetText()
        {
            foreach (var jsonName in _jsonNames)
            {
                yield return Language.GetText(jsonName);
            }
        }
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
        }
    }
}