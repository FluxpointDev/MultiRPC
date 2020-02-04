using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace MultiRPC.Core.Notification
{
    /// <summary>
    /// Where all the logic for logging is contained
    /// </summary>
    public class NotificationCenter
    {
        static NotificationCenter() 
        {
            AddLoggerSinks();
        }

        /// <summary>
        /// Fires when a new <see cref="NotificationToast"/> has been made
        /// </summary>
        public static event EventHandler<NotificationToast> NewNotificationToast;

        private static readonly List<ILogEventSink> Sinks = new List<ILogEventSink>();

        /// <summary>
        /// Logger to be used for... logging
        /// </summary>
        [NotNull]
        public static Logger Logger { get; private set; }

        private static readonly LoggingLevelSwitch LoggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);

        /// <summary>
        /// Level that <see cref="Logger"/> is running at
        /// </summary>
        public static LogEventLevel LoggingLevel
        {
            get => LoggingLevelSwitch.MinimumLevel;
            set => LoggingLevelSwitch.MinimumLevel = value;
        }

        //ToDo: Find why .WriteTo.File is not outputting anything
        /// <summary>
        /// Add additional <see cref="ILogEventSink"/>s to the logger
        /// </summary>
        /// <param name="sinks"><see cref="ILogEventSink"/>s to add to the logger</param>
        public static void AddLoggerSinks(params ILogEventSink[] sinks)
        {
            Logger?.Dispose();
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .WriteTo.File(FileLocations.ErrorFileLocation);

            foreach (var sink in Sinks)
            {
                loggerConfig.WriteTo.Sink(sink);
            }

            foreach (var sink in sinks)
            {
                if (!Sinks.Contains(sink))
                {
                    Sinks.Add(sink);
                    loggerConfig.WriteTo.Sink(sink);
                }
            }

#if DEBUG
            loggerConfig
            .WriteTo.Debug()
            .WriteTo.Console();
#endif

            Logger = loggerConfig.CreateLogger();
        }

        internal static void NewNotificationToastPush(NotificationToast notificationToast)
        {
            NewNotificationToast?.Invoke(null, notificationToast);
        }
    }
}
