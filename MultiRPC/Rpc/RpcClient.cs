using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Message;
using MultiRPC.Extensions;
using MultiRPC.Logging;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI;
using TinyUpdate.Core.Helper;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Rpc
{
    public class RpcClient
    {
        private ILogging _logger = LoggingCreator.CreateLogger(nameof(RpcClient));
        private DateTime _rpcStart;
        private string _presenceName = "Unknown";
        private long _presenceId;
        private DiscordRpcClient? _client;

        public bool IsRunning => Status != ConnectionStatus.Disconnected;
        public long ID => _presenceId;
        public ConnectionStatus Status { get; private set; }

        public event EventHandler<ReadyMessage>? Ready;
        public event EventHandler<ErrorMessage>? Errored;
        public event EventHandler? Disconnected;
        public event EventHandler? Loading;
        public event EventHandler<PresenceMessage>? PresenceUpdated;

        public void ClearPresence()
        {
            _client?.ClearPresence();
        }

        private DisableSettings? _disableSettings;
        private async Task<bool> CheckPresence(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !text.ToLower().Contains("discord.gg"))
            {
                return true;
            }

            _disableSettings ??= SettingManager<DisableSettings>.Setting;
            if (_disableSettings.InviteWarn)
            {
                return true;
            }
            var result = await MessageBox.Show(
                Language.GetText("AdvertisingWarning"), 
                Language.GetText("Warning"),
                MessageBoxButton.OkCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Ok)
            {
                _disableSettings.InviteWarn = true;

                _logger.Warning(Language.GetText("AdvertisingWarningDisabled"));
                return true;
            }

            return result != MessageBoxResult.Cancel;
        }
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeServerProcessId(IntPtr pipe, out int clientProcessId);
        
        private int FindPipe(string? processName)
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
                    using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipe, PipeDirection.InOut, PipeOptions.Asynchronous);
                    pipeClient.Connect(1000);
                    var canGetPipe = GetNamedPipeServerProcessId(pipeClient.SafePipeHandle.DangerousGetHandle(), out var id);
                    pipeClient.Close();
                        
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
                catch { }
            }
            return -1;
        }

        public void Start(long? applicationId, string? applicationName)
        {
            _presenceId = applicationId ?? RpcPageManager.CurrentPage?.RichPresence.ID ?? Constants.MultiRPCID;
            var idS = _presenceId.ToString();

            var name = applicationName ?? RpcPageManager.CurrentPage?.RichPresence.Name ?? Language.GetText("MultiRPC");
            _presenceName = name;
            
            //If we are running already then stop it if we aren't using the same ID
            if (IsRunning && _client?.ApplicationID != idS)
            {
                Stop();
            }
            _logger.Information(Language.GetText("StartingRpc"));

            var processName = SettingManager<GeneralSettings>.Setting.Client switch
            {
                DiscordClient.Discord => "Discord",
                DiscordClient.DiscordPTB => "DiscordPTB",
                DiscordClient.DiscordCanary => "DiscordCanary",
                DiscordClient.DiscordDevelopment => "DiscordDevelopment",
                _ => null
            };
            _client?.Dispose();

            var pipe = FindPipe(processName);
            _logger.Debug($"Discord {Language.GetText("Client")}, Was {(processName ?? Language.GetText("N/A"))} found?: {pipe != -1}");
            _client = new DiscordRpcClient(idS, pipe)
            {
                SkipIdenticalPresence = false, 
                Logger = new RpcLogger(),
                ShutdownOnly = true
            };

            _client.OnPresenceUpdate += (sender, e) => PresenceUpdated?.Invoke(sender, e);
            _client.OnReady += (sender, e) => 
            {
                Status = ConnectionStatus.Connected;
                Ready?.Invoke(sender, e);
            };
            _client.OnError += (sender, e) =>
            {
                Status = ConnectionStatus.Connecting;
                Errored?.Invoke(this, e);
            };
            _rpcStart = DateTime.UtcNow;
            _client.Initialize();

            Status = ConnectionStatus.Connecting;
            Loading?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            _logger.Information(Language.GetText("ShuttingDown"));
            ClearPresence();
            _client?.Deinitialize();
            _client?.Dispose();
            Status = ConnectionStatus.Disconnected;
            _presenceId = 0;
            _presenceName = "Unknown";

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdatePresence(DiscordRPC.RichPresence richPresence)
        {
            //Check that the presence isn't doing any advertising
            if (!await CheckPresence(richPresence.Details) || !await CheckPresence(richPresence.State))
            {
                Stop();
                return;
            }
            
            if (_client?.IsDisposed ?? true)
            {
                return;
            }
            
            _client?.SetPresence(richPresence);
        }
        
        public async Task UpdatePresence(RichPresence? richPresence)
        {
            if (richPresence == null)
            {
                return;
            }
            
            var pre = richPresence.Presence;
            pre.Buttons = pre.Buttons?.Where(x => !string.IsNullOrWhiteSpace(x.Url) && !string.IsNullOrWhiteSpace(x.Label)).ToArray();
            pre.Timestamps = richPresence.UseTimestamp ? new Timestamps
            {
                Start = _rpcStart
            } : null;
            
            if (richPresence.ID != _presenceId)
            {
                Stop();
                Start(richPresence.ID, richPresence.Name);
            }
            await UpdatePresence(pre);
        }
    }
}