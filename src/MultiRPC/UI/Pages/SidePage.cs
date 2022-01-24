using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MultiRPC.UI.Pages;

public abstract class SidePage : UserControl, ISidePage
{
    public abstract string IconLocation { get; }

    public abstract string LocalizableName { get; }

    public abstract void Initialize(bool loadXaml);
        
    public void Initialize()
    {
        if (!IsInitialized)
        {
            Initialize(true);
        }
    }

    public Color? BackgroundColour { get; protected set; }
    public Thickness ContentPadding { get; protected set; } = new Thickness(10, 10, 10, 0);
}