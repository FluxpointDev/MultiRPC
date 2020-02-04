using Serilog;
using System;

namespace MultiRPC.Core.Extensions
{
    /// <summary>
    /// Extensions for any <see cref="ILogger"/>
    /// </summary>
    public static class ILoggerEx
    {
        /// <inheritdoc cref="ILogger.Error(Exception, string)"/>
        public static void Error(this ILogger logger, Exception exception) =>
            logger.Error(exception.ToString());
    }
}
