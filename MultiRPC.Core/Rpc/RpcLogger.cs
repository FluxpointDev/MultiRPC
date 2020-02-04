using DiscordRPC.Logging;
using JetBrains.Annotations;
using MultiRPC.Core.Notification;
using ILogger = DiscordRPC.Logging.ILogger;

namespace MultiRPC.Core.Rpc
{
    /// <summary>
    /// Allows <see cref="Rpc"/> to do logging to <see cref="NotificationCenter"/>
    /// </summary>
    public class RpcLogger : ILogger
    {
        private RpcLogger() { }

        /// <summary>
        /// Logger that <see cref="Rpc.Client"/> should use
        /// </summary>
        public static RpcLogger Current { get; } = new RpcLogger();

        /// <summary>
        /// See <see cref="NotificationCenter.LoggingLevel"/>
        /// </summary>
        public LogLevel Level
        {
            get => NotificationCenter.LoggingLevel.ToRpcLogLevel();
            set => NotificationCenter.LoggingLevel = value.ToSerilogLogLevel();
        }

        public void Error([NotNull] string message, params object[] args)
        {
            NotificationCenter.Logger.Error(message, args);
        }

        public void Info([NotNull] string message, params object[] args)
        {
            NotificationCenter.Logger.Information(message, args);
        }

        public void Trace([NotNull] string message, params object[] args)
        {
            NotificationCenter.Logger.Verbose(message, args);
        }

        public void Warning([NotNull] string message, params object[] args)
        {
            NotificationCenter.Logger.Warning(message, args);
        }
    }

    public static class RpcLoggerEx 
    {
        /// <summary>
        /// Allows <see cref="Serilog.Events.LogEventLevel"/> to <see cref="LogLevel"/> conversion
        /// </summary>
        /// <param name="eventLevel"><see cref="Serilog.Events.LogEventLevel"/> to use</param>
        public static LogLevel ToRpcLogLevel(this Serilog.Events.LogEventLevel eventLevel) =>
            eventLevel switch
            {
                Serilog.Events.LogEventLevel.Debug => LogLevel.Trace,
                Serilog.Events.LogEventLevel.Error => LogLevel.Error,
                Serilog.Events.LogEventLevel.Fatal => LogLevel.Error,
                Serilog.Events.LogEventLevel.Information => LogLevel.Info,
                Serilog.Events.LogEventLevel.Verbose => LogLevel.Trace,
                Serilog.Events.LogEventLevel.Warning => LogLevel.Warning,
                _ => throw new System.NullReferenceException($"{nameof(eventLevel)} is null")
            };

        /// <summary>
        /// Allows <see cref="LogLevel"/> to <see cref="Serilog.Events.LogEventLevel"/> conversion
        /// </summary>
        /// <param name="eventLevel"><see cref="Serilog.Events.LogEventLevel"/> to use</param>
        public static Serilog.Events.LogEventLevel ToSerilogLogLevel(this LogLevel eventLevel) =>
            eventLevel switch
            {
                LogLevel.Error => Serilog.Events.LogEventLevel.Error,
                LogLevel.Info => Serilog.Events.LogEventLevel.Information,
                LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
                LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
                _ => throw new System.NullReferenceException($"{nameof(eventLevel)} is null")
            };
    }
}
