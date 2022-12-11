namespace MultiRPC.Extensions;

public static class CheckResultExt
{
    public static CheckResult Check(this string s) => Check(s, 128);
    public static CheckResult Check(this string s, int max)
    {
        if (s.Length == 1)
        {
            return new CheckResult(false, Language.GetText(LanguageText.OneChar));
        }

        return s.CheckBytes(max)
            ? new CheckResult(true)
            : new CheckResult(false, Language.GetText(LanguageText.TooManyChars));
    }
    
    public static CheckResult CheckUrl(this string s, int byteCount = 512)
    {
        if (string.IsNullOrWhiteSpace(s) || Uri.TryCreate(s, UriKind.Absolute, out _))
        {
            return s.CheckBytes(byteCount)
                ? new CheckResult(true)
                : new CheckResult(false, Language.GetText(LanguageText.UrlTooBig));
        }

        return new CheckResult(false, Language.GetText(LanguageText.InvalidUrl));
    }
}