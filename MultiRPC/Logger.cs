using MultiRPC.GUI;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MultiRPC
{
    /// <summary>
    /// Console logging
    /// </summary>
    public class Logger
    {
        BlockingCollection<LogEvent> bc = new BlockingCollection<LogEvent>();
        private readonly string Name;
        public Logger(string name)
        {
            Name = name;
            Task.Factory.StartNew(() =>
            {
                foreach (LogEvent p in bc.GetConsumingEnumerable())
                {
                    MainWindow.WD.Logs.Dispatcher.Invoke(() =>
                    {
                        MainWindow.WD.Logs.AppendText($"\n[{p.Title}] {p.Message}");
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
        public void Write(string message)
        {
            bc.Add(new LogEvent(Name, message));
        }

        /// <summary>
        /// [Discord] Text + Color Custom
        /// </summary>
        public void Discord(string message)
        {
            bc.Add(new LogEvent("Discord", message));
        }

        public void Error(Exception ex)
        {
            bc.Add(new LogEvent($"{Name} Error", ex.Message));
        }

        public void ImageError(BitmapImage img, ExceptionEventArgs ex)
        {
            bc.Add(new LogEvent("Image Error", $"Failed to download ({img.UriSource.AbsoluteUri}) {ex.ErrorException.Message}"));
        }

        public void Error(string message)
        {
            bc.Add(new LogEvent($"{Name} Error", message));
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
