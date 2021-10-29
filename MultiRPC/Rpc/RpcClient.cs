using System;
using System.Linq;
using DiscordRPC;
using DiscordRPC.Message;
using MultiRPC.Extensions;
using MultiRPC.Logging;
using MultiRPC.Rpc.Page;

namespace MultiRPC.Rpc
{
    public class RpcClient
    {
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

        public void Start(long? applicationId, string? applicationName)
        {
            _presenceId = applicationId ?? RpcPageManager.CurrentPage?.RichPresence.ID ?? Constants.MultiRPCID;
            var idS = _presenceId.ToString();

            var name = applicationName ?? RpcPageManager.CurrentPage?.RichPresence.Name ?? Language.GetText("MultiRPC");
            _presenceName = name!;
            
            //If we are running already then stop it if we aren't
            //using the same ID
            if (IsRunning && _client?.ApplicationID != idS)
            {
                Stop();
            }

            _client?.Dispose();
            _client = new DiscordRpcClient(idS) //TODO: Add custom pipe support
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
            ClearPresence();
            _client?.Deinitialize();
            _client?.Dispose();
            Status = ConnectionStatus.Disconnected;
            _presenceId = 0;
            _presenceName = "Unknown";

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePresence(DiscordRPC.RichPresence richPresence)
        {
            if (_client?.IsDisposed ?? true)
            {
                return;
            }
            
            _client?.SetPresence(richPresence);
        }
        
        public void UpdatePresence(RichPresence? richPresence)
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
            UpdatePresence(pre);
        }
    }
}