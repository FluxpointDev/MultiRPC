using Avalonia;
using Avalonia.Media;

namespace MultiRPC.UI.Pages;

public interface ISidePage
{
    string IconLocation { get; }

    string LocalizableName { get; }

    string? BackgroundResourceName { get; }
    Color? PageBackground { get; }

    bool IsInitialized { get; }

    Thickness ContentPadding { get; }
    
    void Initialize(bool loadXaml);

    public void Initialize();
}