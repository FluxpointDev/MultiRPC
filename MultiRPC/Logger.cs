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
        public Logger()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (LogEvent p in bc.GetConsumingEnumerable())
                {
                    MultiRPC.App.WD.Logs.Dispatcher.Invoke(() =>
                    {
                        MultiRPC.App.WD.Logs.AppendText($"\n[{p.Title}] {p.Message}");
                    });
                }
            });
        }

        ~Logger()
        {
            bc.CompleteAdding();
        }

        /// <summary> [App] Text </summary>
        public void App(string message)
        {
            bc.Add(new LogEvent("App", message));
        }

        /// <summary> [RPC] Text </summary>
        public void Rpc(string message)
        {
            bc.Add(new LogEvent("RPC", message));
        }

        /// <summary> [Discord] Text </summary>
        public void Discord(string message)
        {
            bc.Add(new LogEvent("Discord", message));
        }

        /// <summary> [Custom Error] Error message </summary>
        public void Error(string name, string message)
        {
            bc.Add(new LogEvent($"{name} Error", message));
        }

        /// <summary> [Custom Error] Exception error </summary>
        public void Error(string name, Exception ex)
        {
            bc.Add(new LogEvent($"{name} Error", ex.Message));
        }

        /// <summary> [Image Error] Failed to download </summary>
        public void ImageError(BitmapImage img, ExceptionEventArgs ex)
        {
            if (ex == null)
                bc.Add(new LogEvent("Image Error", $"Failed to download ({img.UriSource.AbsoluteUri}) Network error"));
            else
                bc.Add(new LogEvent("Image Error", $"Failed to download ({img.UriSource.AbsoluteUri}) {ex.ErrorException.Message}"));
        }

        /// <summary> Log message class </summary>
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
