using Avalonia.Controls;
using Avalonia.Media;
using MultiRPC.Logging;

namespace MultiRPC.UI.Pages;

public class LoggingPage : StackPanel, ISidePage
{
    public string IconLocation => "Icons/Logging";
    public string LocalizableName => "Log";
    public Color? PageBackground { get; } = Colors.Black;
    public new double Height => 550;

    public void Initialize(bool loadXaml)
    {
        Spacing = 5;
        LoggingPageLogger.AddAction(log => Children.Add(log));
    }
}