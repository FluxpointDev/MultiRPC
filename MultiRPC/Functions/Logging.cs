using System;
using DiscordRPC.Logging;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace MultiRPC.Functions
{
    public class Logging : INotifyPropertyChanged, ILogger
    {
        public Logging()
        {
            Application(App.Text.LoggerHasStarted);
        }

        private string logText;
        public string LogText
        {
            set
            {
                if (logText != value)
                {
                    logText = value;
                    OnPropertyChanged("LogText");
                }
            }
            get => logText;
        }

#if DEBUG
        private LogLevel _level = LogLevel.Trace;
#elif !DEBUG
        private LogLevel _level = LogLevel.Info;
#endif
        public LogLevel Level
        {
            get => _level;
            set => _level = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary> [App] Text </summary>
        public void Application(string message)
        {
            LogText += LogEvent("App", message);
        }

        /// <summary> [Discord] Text </summary>
        public void Discord(string message)
        {
            LogText += LogEvent("Discord", message);
        }

        /// <summary> [Custom Error] Error message </summary>
        public void Error(string name, string message)
        {
            LogText += LogEvent($"{name} {App.Text.Error}", message);
        }

        /// <summary> [Custom Error] Exception error </summary>
        public void Error(string name, Exception ex)
        {
            LogText += LogEvent($"{name} {App.Text.Error}", ex.Message);
        }

        /// <summary> [Image Error] Failed to download </summary>
        public void ImageError(BitmapImage img, ExceptionEventArgs ex)
        {
            if (ex == null)
                LogText += LogEvent(App.Text.ImageError, $"{App.Text.FailedToDownload} ({img.UriSource.AbsoluteUri}) {App.Text.NetworkError}");
            else
                LogText += LogEvent(App.Text.ImageError, $"{App.Text.FailedToDownload} ({img.UriSource.AbsoluteUri}) {ex.ErrorException.Message}");
        }

        public string LogEvent(string name, string message)
        {
            return $"[{name}]: {message}\r\n\r\n";
        }

        public void Trace(string message, params object[] args)
        {
            LogText += LogEvent($"RPC {App.Text.Trace}", RPCMessage(message, args));
        }

        public void Info(string message, params object[] args)
        {
            LogText += LogEvent($"RPC {App.Text.Info}", RPCMessage(message, args));
        }

        public void Warning(string message, params object[] args)
        {
            LogText += LogEvent($"RPC {App.Text.Warning}", RPCMessage(message, args));
        }

        public void Error(string message, params object[] args)
        {
            LogText += LogEvent($"RPC {App.Text.Error}", RPCMessage(message, args));
        }

        private string RPCMessage(string message, params object[] args)
        {
           return message + (args != null && args.Length > 0 ? "\r\nArgs\r\n" + string.Join("\r\n", args) : "");
        }
    }
}
