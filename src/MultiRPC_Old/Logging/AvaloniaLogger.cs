using Avalonia.Logging;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Logging;

public class AvaloniaLogger : ILogSink
{
    private readonly Dictionary<string, ILogging> _loggers = new Dictionary<string, ILogging>();
    private readonly string[] _areas;
    public AvaloniaLogger(params string[] areas)
    {
        _areas = areas;
    }
    
    public bool IsEnabled(LogEventLevel level, string area)
    {
        var logLevel = ToLogLevel(level);
        //If we give any areas to look at then only log them
        if (_areas.Any() 
            && _areas.Contains(area)
            && logLevel is LogLevel.Info or LogLevel.Trace)
        {
            return false;
        }
        
        return LoggingCreator.ShouldProcess(null, logLevel);
    }

    private static LogLevel ToLogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Information => LogLevel.Info,
            LogEventLevel.Warning => LogLevel.Warn,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Error,
            _ => LogLevel.Trace
        };
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        Log(level, area, source, messageTemplate, Array.Empty<object>());
    }

    public void Log<T0>(LogEventLevel level, string area, object? source, string messageTemplate, T0? propertyValue0)
    {
        Log(level, area, source, messageTemplate, new object?[] { propertyValue0 });
    }

    public void Log<T0, T1>(LogEventLevel level, string area, object? source, string messageTemplate, T0? propertyValue0,
        T1? propertyValue1)
    {
        Log(level, area, source, messageTemplate, new object?[] { propertyValue0, propertyValue1 });
    }

    public void Log<T0, T1, T2>(LogEventLevel level, string area, object? source, string messageTemplate, T0? propertyValue0,
        T1? propertyValue1, T2? propertyValue2)
    {
        Log(level, area, source, messageTemplate, new object?[] { propertyValue0, propertyValue1, propertyValue2 });
    }

    public void Log(LogEventLevel level, string area, object? source, string message, object?[] propertyValues)
    {
        var messageTemplate = FormatMessage(message, out var correctlyProcessedMessage);

        if (!_loggers.ContainsKey(area))
        {
            _loggers[area] = LoggingCreator.CreateLogger(nameof(AvaloniaLogger) + " (" + area + ")");
        }
        var logger = _loggers[area];

        //If we fail to format the message then do this so we don't crash
        if (!correctlyProcessedMessage)
        {
            logger.Warning("Wasn't able to process avalonia logging properly!");
            propertyValues = Array.Empty<object>();
            messageTemplate = message;
        }
        //TODO: Use source + area
        switch (ToLogLevel(level))
        {
            case LogLevel.Trace:
                logger.Debug(messageTemplate, propertyValues);
                break;
            case LogLevel.Info:
                logger.Information(messageTemplate, propertyValues);
                break;
            case LogLevel.Warn:
                logger.Warning(messageTemplate, propertyValues);
                break;
            case LogLevel.Error:
                logger.Error(messageTemplate, propertyValues);
                break;
        }
    }

    private string FormatMessage(string message, out bool correctlyProcessedMessage)
    {
        var properties = new List<string>();
        var tmpMes = message;
        var formattedMessage = message;

        var startIndex = message.IndexOf('{');
        var endIndex = message.IndexOf('}');
        while (startIndex != -1 && endIndex != -1)
        {
            endIndex++;
            var prop = tmpMes[startIndex..endIndex];
            if (!properties.Contains(prop))
            {
                properties.Add(prop);
            }
            var ind = properties.IndexOf(prop);
            tmpMes = tmpMes[endIndex..];
            formattedMessage = formattedMessage[..formattedMessage.IndexOf(prop, StringComparison.Ordinal)] + $"{{{ind}}}" + tmpMes;

            startIndex = tmpMes.IndexOf('{');
            endIndex = tmpMes.IndexOf('}');
        }

        correctlyProcessedMessage = !tmpMes.Contains('{') && !tmpMes.Contains('}');
        return formattedMessage;
    }
}