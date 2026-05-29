namespace MultiRPC.UI.Pages;

public static class PageManager
{
    private static readonly List<ISidePage> Pages = new List<ISidePage>();
        
    public static void AddPage(ISidePage page)
    {
        Pages.Add(page);
        PageAdded?.Invoke(page, page);
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public static IReadOnlyList<ISidePage> CurrentPages => Pages.AsReadOnly();

    public static event EventHandler<ISidePage>? PageAdded;
}