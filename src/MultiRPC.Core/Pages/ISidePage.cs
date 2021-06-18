namespace MultiRPC.Core.Pages
{
    /// <summary>
    /// Page that lives in the sidebar
    /// </summary>
    public interface ISidePage : IRequired
    {
        string IconLocation { get; }

        string LocalizableName { get; }
    }
}
