using Avalonia;
using Avalonia.Media;

namespace MultiRPC.UI.Pages;

public interface ISidePage
{
    string IconLocation { get; }

    string LocalizableName { get; }

    string? BackgroundResourceName => null;
    Color? PageBackground => null;
    double Height => double.NaN;

    bool IsInitialized { get; }

    Thickness ContentPadding => new Thickness(10, 10, 10, 0);
    
    void Initialize(bool loadXaml);

    public void Initialize()
    {
        if (!IsInitialized)
        {
            Initialize(true);
        }
    }
}