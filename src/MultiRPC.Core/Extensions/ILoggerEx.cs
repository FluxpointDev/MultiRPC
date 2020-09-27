using JetBrains.Annotations;
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
        public static void Error([NotNull] this ILogger logger, [NotNull] Exception exception) =>
            logger.Error(exception.Message);
    }
}
