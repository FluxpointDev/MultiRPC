using System;
using System.Collections.Generic;
using Avalonia.Controls;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Pages
{
    public partial class LoggingPage : SidePage
    {
        public LoggingPage() { }

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

    public class LoggingPageLogger : ILogging
    {
        internal static void AddAction(Action<TextBlock> action)
        {
            _action = action;
            foreach (var log in StoredLogging)
            {
                _action.Invoke(log);
            }
        }

        private static Action<TextBlock>? _action;
        private static readonly List<TextBlock> StoredLogging = new List<TextBlock>();
        
        public LoggingPageLogger(string name) => Name = name;

        public void Debug(string message, params object?[] propertyValues)
        {
            if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Trace))
            {
                WriteLog(message, "Debug", propertyValues);
            }
        }

        public void Information(string message, params object?[] propertyValues)
        {
            if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Info))
            {
                WriteLog(message, "Info", propertyValues);
            }
        }

        public void Warning(string message, params object?[] propertyValues)
        {
            if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Warn))
            {
                WriteLog(message, "Warn", propertyValues);
            }
        }

        public void Error(string message, params object?[] propertyValues)
        {
            if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Error))
            {
                WriteLog(message, "Error", propertyValues);
            }
        }

        public void Error(Exception e, params object?[] propertyValues)
        {
            Error(e.Message, propertyValues);
        }

        private void WriteLog(string message, string type, params object?[] propertyValues)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var textBlock = new TextBlock()
                {
                    Text = $"[{type} - {Name}]: " + string.Format(message, propertyValues)
                };

                if (_action != null)
                {
                    _action.Invoke(textBlock);
                    return;
                }
                StoredLogging.Add(textBlock);
            });
        }

        public string Name { get; }
        public LogLevel? LogLevel { get; set; }
    }

    public class LoggingPageBuilder : LoggingBuilder
    {
        public override ILogging CreateLogger(string name)
        {
            return new LoggingPageLogger(name);
        }
    }
}