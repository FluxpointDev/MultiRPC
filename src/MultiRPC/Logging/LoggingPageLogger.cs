using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Logging;

public class LoggingPageLogger : ILogging
{
    private static Action<TextBlock>? _action;
    private static readonly List<string[]> StoredLogging = new List<string[]>();
    private static readonly Language DebugLanguage = LanguageText.Debug;
    private static readonly Language InfoLanguage = LanguageText.Info;
    private static readonly Language WarnLanguage = LanguageText.Warn;
    private static readonly Language ErrorLanguage = LanguageText.Error;

    internal static void AddAction(Action<TextBlock> action)
    {
        _action = action;
        foreach (var log in StoredLogging)
        {
            _action.Invoke(MakeTextBlock(log[0], log[1]));
        }
    }
    
    public LoggingPageLogger(string name) => Name = name;

    public void Debug(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Trace))
        {
            WriteLog(message, DebugLanguage.Text, propertyValues);
        }
    }

    public void Information(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Info))
        {
            WriteLog(message, InfoLanguage.Text, propertyValues);
        }
    }

    public void Warning(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Warn))
        {
            WriteLog(message, WarnLanguage.Text, propertyValues);
        }
    }

    public void Error(string message, params object?[] propertyValues)
    {
        if (LoggingCreator.ShouldProcess(LogLevel, TinyUpdate.Core.Logging.LogLevel.Error))
        {
            WriteLog(message, ErrorLanguage.Text, propertyValues);
        }
    }

    public void Error(Exception e, params object?[] propertyValues)
    {
        Error(e.Message, propertyValues);
    }

    private void WriteLog(string message, string type, params object?[] propertyValues)
    {
        var header = $"[{type} - {Name}]:";
        var mess = string.Format(message, propertyValues);
        if (_action != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _action.Invoke(MakeTextBlock(header, mess));
            });
            return;
        }
        StoredLogging.Add(new [] { header, mess });
    }

    private static TextBlock MakeTextBlock(string header, string message)
    {
        return new TextBlock()
        {
            Text = header + " " + message,
            Foreground = Brushes.White
        };
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