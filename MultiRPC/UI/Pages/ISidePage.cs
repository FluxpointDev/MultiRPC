using Avalonia;
using Avalonia.Media;

namespace MultiRPC.UI.Pages;

public interface ISidePage
{
    string IconLocation { get; }

    string LocalizableName { get; }

    void Initialize(bool loadXaml);

    public void Initialize();

    Color? BackgroundColour { get; }

    bool IsInitialized { get; }

    Thickness ContentPadding { get; }
}