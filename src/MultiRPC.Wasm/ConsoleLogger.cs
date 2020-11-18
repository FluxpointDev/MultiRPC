using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Uno.Foundation;

namespace MultiRPC.Wasm
{
    public class ConsoleLogger : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public ConsoleLogger(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            switch (logEvent.Level)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    WebAssemblyRuntime.InvokeJS($"console.log(\"{message}\")");
                    break;
                case LogEventLevel.Information:
                    WebAssemblyRuntime.InvokeJS($"console.info(\"{message}\")");
                    break;
                case LogEventLevel.Warning:
                    WebAssemblyRuntime.InvokeJS($"console.warn(\"{message}\")");
                    break;
                case LogEventLevel.Error:
                    WebAssemblyRuntime.InvokeJS($"console.error(\"{message}\")");
                    break;
                case LogEventLevel.Fatal:
                    WebAssemblyRuntime.InvokeJS($"console.error(\"FATAL ERROR: {message}\")");
                    break;
            }
        }
    }

    public static class MySinkExtensions
    {
        public static LoggerConfiguration ConsoleLogger(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new ConsoleLogger(formatProvider));
        }
    }
}
