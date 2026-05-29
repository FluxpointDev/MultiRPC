using DiscordRPC.Logging;
using TinyUpdate.Core.Logging;
using LogLevel = DiscordRPC.Logging.LogLevel;

namespace MultiRPC.Logging;

public class RpcLogger : ILogger
{
    private readonly ILogging _internalLogger = LoggingCreator.CreateLogger(nameof(RpcLogger));
        
    public void Trace(string message, params object[] args)
    {
        _internalLogger.Debug(message, args);
    }

    public void Info(string message, params object[] args)
    {
        _internalLogger.Information(message, args);
    }
        
    public void Warning(string message, params object[] args)
    { 
        _internalLogger.Warning(message, args);
    }

    public void Error(string message, params object[] args)
    { 
        _internalLogger.Error(message, args);
    }

    public LogLevel Level
    {
        get => LoggingCreator.GlobalLevel.ToRpcLogLevel();
        set
        {
            //Don't do anything, LogLevel is control at LoggingCreator.GlobalLevel
        }
    }
}

public static class RpcLoggerExt
{
    public static LogLevel ToRpcLogLevel(this TinyUpdate.Core.Logging.LogLevel logLevel)
    {
        return logLevel switch
        {
            TinyUpdate.Core.Logging.LogLevel.Trace => LogLevel.Trace,
            TinyUpdate.Core.Logging.LogLevel.Info => LogLevel.Info,
            TinyUpdate.Core.Logging.LogLevel.Warn => LogLevel.Warning,
            TinyUpdate.Core.Logging.LogLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
    }
}