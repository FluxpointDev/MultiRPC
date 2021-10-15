using System;
using DiscordRPC;
using DiscordRPC.Message;

namespace MultiRPC.Rpc
{
    public class RpcClient
    {
        private DiscordRpcClient? _client;

        public bool IsRunning => Status != ConnectionStatus.Disconnected;

        public ConnectionStatus Status { get; private set; }

        private string _presenceName = "Unknown";
        private long _presenceId;
        public RichPresence? ActivePresence => _client?.CurrentPresence != null ? 
            null 
            : new RichPresence(_presenceName, _presenceId)
            {
                Presence = _client?.CurrentPresence!
            };

        public event EventHandler<ReadyMessage>? Ready;
        public event EventHandler<ErrorMessage>? Errored;
        public event EventHandler? Disconnected;
        public event EventHandler? Loading;
        public event EventHandler<RichPresence?>? PresenceUpdated;

        public void ClearPresence()
        {
            _client?.ClearPresence();
        }

        public void Start(long? applicationId, string? applicationName)
        {
            var id = applicationId ?? RpcPageManager.CurrentPage?.RichPresence.ID;
            var idS = id.ToString();
            var name = applicationName ?? RpcPageManager.CurrentPage?.RichPresence.Name;
            
            //If we are running already then stop it if we aren't
            //using the same ID
            if (IsRunning && _client?.ApplicationID != idS)
            {
                Stop();
            }
            _presenceId = id.GetValueOrDefault(Constants.MultiRPCID);
            _presenceName = name!;

            _client = new DiscordRpcClient(idS) //TODO: Add custom pipe support
            {
                SkipIdenticalPresence = false, 
                //Logger = new RpcLogger(), 
                ShutdownOnly = true
            };

            UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
            _client.OnPresenceUpdate += (sender, e) =>
            {
                PresenceUpdated?.Invoke(sender, ActivePresence);
            };
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
            _presenceName = "";

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePresence(RichPresence? richPresence)
        {
            if (richPresence == null)
            {
                return;
            }
            _presenceId = richPresence.ID;
            _presenceName = richPresence.Name;
            
            if (!_client?.IsDisposed ?? false) 
            {
                //TODO: Check ID is the same and restart if needed
                _client?.SetPresence(richPresence.Presence);
            }
        }
    }
}