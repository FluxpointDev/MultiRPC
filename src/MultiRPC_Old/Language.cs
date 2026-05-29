using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using MultiRPC.Converters;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC;

public static class LanguageGrab
{
    internal static Dictionary<LanguageText, string> EnglishLanguage;
    internal static Dictionary<LanguageText, string>? CurrentLanguage;
    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(LanguageGrab));
    private static readonly LanguageFileContext SerializerOptions = new LanguageFileContext(new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
        //We know that the file in question is safe as we provide the files 
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters =
        {
            new DictionaryTKeyEnumTValueConverter<LanguageText, string>(),
        }
    });

    static LanguageGrab()
    {
        GrabLanguage();
    }
    
    public static event EventHandler? LanguageChanged;
    
    [MemberNotNull(nameof(EnglishLanguage))]
    private static void GrabLanguage()
    {
        EnglishLanguage = GrabLanguageFile(GetFilePath("en-gb"))!;

        //If we have a set language then we want to load the current languages we have
        var langName = SettingManager<GeneralSettings>.Setting.Language;
        if (!string.IsNullOrWhiteSpace(langName))
        {
            //Only load it in if we have the language
            GeneralSettings.GetLanguages();
            if (GeneralSettings.Languages.ContainsKey(langName))
            {
                CurrentLanguage = GrabLanguageFile(GeneralSettings.Languages[langName]);
            }
        }

        if (CurrentLanguage != null)
            return;

        /*If we wasn't able to load the users language from
         settings in, try to load in from what the system is*/
        var currentCulture = Thread.CurrentThread.CurrentUICulture;
        var currentLang = currentCulture.Name.ToLower();
            
        //Don't try if the system is en-gb as we already load that in as a fallback
        if (currentLang != "en-gb")
        {
            var currentLangTwoLetter = currentCulture.TwoLetterISOLanguageName.ToLower();
            CurrentLanguage =
                GrabLanguageFile(GetFilePath(currentLang)) 
                ?? GrabLanguageFile(GetFilePath(currentLangTwoLetter));
        }
    }

    private static string GetFilePath(string name) => Path.Combine(Constants.LanguageFolder, name + ".json");
    private static Dictionary<LanguageText, string>? GrabLanguageFile(string fileLocation)
    {
        if (!File.Exists(fileLocation))
        {
            return null;
        }
           
        try
        {
            return JsonSerializer.Deserialize(File.ReadAllText(fileLocation), 
                (JsonTypeInfo<Dictionary<LanguageText, string>>)SerializerOptions.GetTypeInfo(typeof(Dictionary<LanguageText, string>)));
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        return null;
    }

    internal static void ChangeLanguage(string file)
    {
        var lan = GrabLanguageFile(file);
        if (lan != null)
        {
            CurrentLanguage = lan;
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}

public class Language
{
    private readonly Lazy<LanguageObservable> _textObservable;
    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Language));
    private static readonly List<Language> CachedLanguages = new List<Language>();
    private LanguageText?[]? _jsonNames;

    public Language() : this(LanguageText.NA) { }
    public Language(params string[] jsonNames) : this(jsonNames.Select(GetLanguageText).ToArray()) { }
    public Language(params LanguageText?[] jsonNames)
    {
        _jsonNames = jsonNames;
        _textObservable = new Lazy<LanguageObservable>(() =>
        {
            _jsonNames = null;
            return new LanguageObservable(jsonNames);
        });
    }

    
    public LanguageObservable TextObservable => _textObservable.Value;
    public string Text
    {
        get
        {
            if (!_textObservable.IsValueCreated && _jsonNames != null)
            {
                return _jsonNames.Length == 1 ? 
                    GetText(_jsonNames[0]) :
                    string.Join(' ', _jsonNames.Select(GetText));
            }
            return TextObservable.Text;
        }
    }


    public void ChangeJsonNames(params string[] jsonNames) => ChangeJsonNames(jsonNames.Select(GetLanguageText).ToArray());
    public void ChangeJsonNames(params LanguageText?[] jsonNames) => _textObservable.Value.ChangeJsonNames(jsonNames);
    
    
    public static Language GetLanguage(string languageText) => GetLanguage(GetLanguageText(languageText));
    public static Language GetLanguage(LanguageText? languageText)
    {
        languageText ??= LanguageText.NA;
        var lang = CachedLanguages.FirstOrDefault(x => 
            (x._textObservable.IsValueCreated ? 
                x._textObservable.Value.JsonNames : 
                (x._jsonNames ?? x._textObservable.Value.JsonNames))
            .Any(y => y == languageText));
        if (lang == null)
        {
            lang = new Language((LanguageText)languageText);
            CachedLanguages.Add(lang);
        }

        return lang;
    }


    public static string GetText(string? langText) => GetText(GetLanguageText(langText));
    public static string GetText(LanguageText? jsonName)
    {
        if (jsonName == null)
        {
            return GetText(LanguageText.NA);
        }
        if (LanguageGrab.CurrentLanguage?.ContainsKey((LanguageText)jsonName) ?? false)
        {
            return LanguageGrab.CurrentLanguage[(LanguageText)jsonName];
        }
        //We manually have the N/A here in case it isn't seen anywhere (But that would also show a bigger issue)
        return LanguageGrab.EnglishLanguage.ContainsKey((LanguageText)jsonName) ?
            LanguageGrab.EnglishLanguage[(LanguageText)jsonName] : 
            GetText(LanguageText.NA);
    }

    public static bool HasKey(string? jsonName) => GetLanguageText(jsonName) != null;

    private static LanguageText? GetLanguageText(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return null;
        }

        try
        {
            return Enum.Parse<LanguageText>(lang);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }

        return null;
    }

    public override string ToString() => Text;
    public static implicit operator Language(LanguageText b) => GetLanguage(b);
    public static implicit operator Language(string b) => GetLanguage(b);
}

public class LanguageObservable : IObservable<string>
{
    internal LanguageText?[] JsonNames;
    private readonly List<IObserver<string>> _observables = new List<IObserver<string>>();

    public LanguageObservable(params LanguageText?[] jsonNames)
    {
        JsonNames = jsonNames;
    }

    internal string Text => string.Join(' ', GetText());

    public IDisposable Subscribe(IObserver<string> observer)
    {
        LanguageGrab.LanguageChanged += LanguageOnLanguageChanged;
        _observables.Add(observer);
        observer.OnNext(Text);
        return Disposable.Create(() => _observables.Remove(observer));
    }

    private void LanguageOnLanguageChanged(object? sender, EventArgs e)
    {
        _observables.ForEach(x => x.OnNext(Text));
    }

    private IEnumerable<string> GetText() => JsonNames.Select(Language.GetText);
    
    internal void ChangeJsonNames(params LanguageText?[] jsonNames)
    {
        JsonNames = jsonNames;
        _observables.ForEach(x => x.OnNext(Text));
    }
}