using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DiscordRPC.Logging;
using MultiRPC.GUI.Pages;

namespace MultiRPC.Functions
{
    public class Logging : INotifyPropertyChanged, ILogger
    {
        public Logging()
        {
            Application(App.Text.LoggerHasStarted);
        }

        private string _logText;

        public string LogText
        {
            set
            {
                if (_logText != value)
                {
                    _logText = value;
                    OnPropertyChanged("LogText");
                }
            }
            get => _logText;
        }

#if DEBUG
        private static readonly LogLevel _level = LogLevel.Trace;
#else
        private static readonly LogLevel _level = LogLevel.Info;
#endif
        public LogLevel Level { get; set; } = _level;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
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
            {
                LogText += LogEvent(App.Text.ImageError,
                    $"{App.Text.FailedToDownload} ({img.UriSource.AbsoluteUri}) {App.Text.NetworkError}");
            }
            else
            {
                LogText += LogEvent(App.Text.ImageError,
                    $"{App.Text.FailedToDownload} ({img.UriSource.AbsoluteUri}) {ex.ErrorException.Message}");
            }
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
            if (args.Contains("Access to the path is denied."))
            {
                MainPage._MainPage.frmRPCPreview.Dispatcher.Invoke(() =>
                {
                    RPC.Shutdown();
                    MessageBox.Show("Your Discord client is running under administrator, MultiRPC needs admin approval to run.\n" +
                        "Go to settings and click on Admin Mode.", "Admin Required", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            LogText += LogEvent($"RPC {App.Text.Error}", RPCMessage(message, args));
        }

        private string RPCMessage(string message, params object[] args)
        {
            return message + (args != null && args.Length > 0 ? "\r\nArgs\r\n" + string.Join("\r\n", args) : "");
        }
    }
}