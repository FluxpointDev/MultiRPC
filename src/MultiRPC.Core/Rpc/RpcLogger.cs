using DiscordRPC.Logging;
using Serilog;

namespace MultiRPC.Core.Rpc
{
    public class RpcLogger : DiscordRPC.Logging.ILogger
    {
        public LogLevel Level { get; set; }

        public void Error(string message, params object[] args)
        {
            Log.Logger.Error(message, args);
        }

        public void Info(string message, params object[] args)
        {
            Log.Logger.Information(message, args);
        }

        public void Trace(string message, params object[] args)
        {
            Log.Logger.Debug(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Log.Logger.Warning(message, args);
        }
    }
}
