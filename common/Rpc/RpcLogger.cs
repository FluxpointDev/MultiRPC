using DiscordRPC.Logging;
using Serilog;
using System;
using System.Diagnostics;

namespace MultiRPC.Common.RPC
{
    public class RpcLogger : DiscordRPC.Logging.ILogger
    {
        public LogLevel Level { get; set; }

        public void Error(string message, params object[] args)
        {
            Log.Logger.Error(message, "Error");
        }

        public void Info(string message, params object[] args)
        {
            Log.Logger.Information(message, "Info");
        }

        public void Trace(string message, params object[] args)
        {
            Log.Logger.Debug(message, "Trace");
        }

        public void Warning(string message, params object[] args)
        {
            Log.Logger.Warning(message, "Warning");
        }
    }
}
