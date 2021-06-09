namespace MultiRPC.Core.Page
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
