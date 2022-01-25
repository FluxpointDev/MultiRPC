using Avalonia.Media;
using MultiRPC.Logging;

namespace MultiRPC.UI.Pages;

public partial class LoggingPage : SidePage
{
    public override string IconLocation => "Icons/Logging";
    public override string LocalizableName => "Log";
    public override Color? PageBackground { get; } = Colors.Black;

    public override void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);
        LoggingPageLogger.AddAction(log =>
        {
            stpLogs.Children.Add(log);
        });
    }
}