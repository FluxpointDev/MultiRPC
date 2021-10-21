using MultiRPC.Logging;

namespace MultiRPC.UI.Pages
{
    public partial class LoggingPage : SidePage
    {
        public override string IconLocation => "Icons/Logging";
        public override string LocalizableName => "Log";
        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);
            LoggingPageLogger.AddAction(log =>
            {
                stpLogs.Children.Add(log);
            });
        }
    }
}