using System.Windows.Documents;
using MultiRPC.Core.Enums;
using Serilog.Events;

namespace MultiRPC.GUI.CorePages
{
    /// <summary>
    /// Interaction logic for LoggingPage.xaml
    /// </summary>
    public partial class LoggingPage : PageWithIcon, Serilog.Core.ILogEventSink
    {
        public override MultiRPCIcons IconName { get; } = MultiRPCIcons.Logs;
        public override string JsonContent => "Log";

        public LoggingPage()
        {
            InitializeComponent();
        }

        public void Emit(LogEvent logEvent)
        {
            //ToDo: Get coloured args
            Dispatcher.InvokeAsync(() =>
            {
                var run = new Run($"[{logEvent.Level.ToString()}]: " + logEvent.RenderMessage() + "\r\n");
                switch (logEvent.Level)
                {
                    case LogEventLevel.Warning:
                        run.SetResourceReference(TextElement.ForegroundProperty, "Orange");
                        break;
                    case LogEventLevel.Fatal:
                    case LogEventLevel.Error:
                        run.SetResourceReference(TextElement.ForegroundProperty, "Red");
                        break;
                };
                txtLogOutput.Inlines.Add(run);
            });
        }
    }
}
