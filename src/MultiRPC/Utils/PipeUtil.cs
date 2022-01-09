using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using TinyUpdate.Core.Helper;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Utils;

public class PipeUtil
{
    private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(PipeUtil));

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetNamedPipeServerProcessId(IntPtr pipe, out int clientProcessId);
        
    public static int FindPipe(string? processName)
    {
        //TODO: Make this work on other OS's
        if (OSHelper.ActiveOS != OSPlatform.Windows
            || string.IsNullOrWhiteSpace(processName))
        {
            return -1;
        }

        var pipeCount = -1;
        var pipes = Directory.GetFiles(@"\\.\pipe\");
        foreach (var t in pipes)
        {
            var pipe = t[9..];
            if (!pipe.StartsWith("discord"))
            {
                continue;
            }

            pipeCount++;
            try
            {
                using NamedPipeClientStream pipeClient =
                    new NamedPipeClientStream(".", pipe, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipeClient.Connect(1000);
                var canGetPipe =
                    GetNamedPipeServerProcessId(pipeClient.SafePipeHandle.DangerousGetHandle(), out var id);
                pipeClient.Dispose();

                if (!canGetPipe || id == 0)
                {
                    continue;
                }

                Process proc = Process.GetProcessById(id);
                if (proc.ProcessName == processName)
                {
                    return pipeCount;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        return -1;
    }
}