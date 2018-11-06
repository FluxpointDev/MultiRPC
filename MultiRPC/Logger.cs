using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MultiRPC
{
    /// <summary>
    /// Console logging
    /// </summary>
    public class Logger
    {
        BlockingCollection<LogEvent> bc = new BlockingCollection<LogEvent>();

        public Logger()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (LogEvent p in bc.GetConsumingEnumerable())
                {
                    MainWindow.WD.View_Logs.Dispatcher.Invoke(() =>
                    {
                        MainWindow.WD.View_Logs.AppendText($"\n[{p.Title}] {p.Message}");
                    });
                }
            });
        }

        ~Logger()
        {
            bc.CompleteAdding();
        }

        /// <summary>
        /// [Discord] Text + Color Custom
        /// </summary>
        public void App(string message)
        {
            bc.Add(new LogEvent("MultiRPC", message));
        }

        /// <summary>
        /// [Discord] Text + Color Custom
        /// </summary>
        public void Program(string message)
        {
            bc.Add(new LogEvent("Program", message));
        }

        /// <summary>
        /// [Discord] Text + Color Custom
        /// </summary>
        public void Discord(string message)
        {
            bc.Add(new LogEvent("Discord", message));
        }

        public void Error(string message)
        {
            bc.Add(new LogEvent("Error", message));
        }

        /// <summary>
        /// Log message
        /// </summary>
        internal class LogEvent
        {
            internal string Title;
            internal string Message;
            public LogEvent(string title, string message)
            {
                Title = title;
                Message = message;
            }
        }
    }
}
