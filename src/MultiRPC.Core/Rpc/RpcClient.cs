using MultiRPC.Core.Rpc;
using System;
using DiscordRPC;
using RichPresence = MultiRPC.Core.Rpc.RichPresence;
using User = MultiRPC.Core.Rpc.User;
using JetBrains.Annotations;

namespace MultiRPC.Core.Rpc
{
    public class RpcClient : IRpcClient
    {
        private DiscordRpcClient Client;

        public bool IsRunning { get; private set; }

        public ConnectionStatus Status { get; private set; }

        public RichPresence ActivePresence { get; private set; }

        public event EventHandler<string> ActivityJoin;
        public event EventHandler<string> ActivitySpectate;
        public event EventHandler<User> ActivityJoinRequest;
        public event EventHandler<Invite> ActivityInvite; //TODO: Find out what I need to do to hook this up
        public event EventHandler Ready;
        public event EventHandler Errored;
        public event EventHandler<bool> Disconnected;
        public event EventHandler Loading;
        public event EventHandler<RichPresence> PresenceUpdated;

        public Result AcceptInvite(long userID)
        {
            throw new NotImplementedException();
        }

        public Result ClearPresence()
        {
            ActivePresence = null;
            Client?.ClearPresence();
            return Result.Success;
        }

        public void RegisterCommand(string command)
        {
            throw new NotImplementedException();
        }

        public void RegisterSteam(int gameID)
        {
            throw new NotImplementedException();
        }

        public Result SendInvite(long userID, ActionType actionType, string message)
        {
            throw new NotImplementedException();
        }

        public Result SendRequestReply(long userID, JoinRequestReply reply)
        {
            throw new NotImplementedException();
        }

        public void Start(long? applicationID)
        {
            if (IsRunning)
            {
                Stop();
            }
            IsRunning = true;

            Client = new DiscordRpcClient((applicationID ?? RpcPageManager.CurrentPage.RichPresence.ApplicationId).ToString()) //TODO: Add custom pipe support
            {
                SkipIdenticalPresence = false,
                Logger = new RpcLogger()
            };
            Client.ShutdownOnly = true;

            UpdatePresence(ActivePresence ?? RpcPageManager.CurrentPage.RichPresence);
            Client.OnPresenceUpdate += (sender, e) =>
            {
                if (ActivePresence.Assets?.LargeImage?.Key != null &&
                    ActivePresence.Assets?.LargeImage?.Key == e.Presence.Assets?.LargeImageKey)
                {
                    ActivePresence.Assets.LargeImage.Uri = new Uri($"https://cdn.discordapp.com/app-assets/{ActivePresence.ApplicationId}/{e.Presence.Assets?.LargeImageID}.png");
                }

                if (ActivePresence.Assets?.SmallImage?.Key != null &&
                    ActivePresence.Assets?.SmallImage?.Key == e.Presence.Assets?.SmallImageKey)
                {
                    ActivePresence.Assets.SmallImage.Uri = new Uri($"https://cdn.discordapp.com/app-assets/{ActivePresence.ApplicationId}/{e.Presence.Assets?.SmallImageID}.png");
                }
                PresenceUpdated?.Invoke(sender, ActivePresence.Clone());
            };
            Client.OnReady += (sender, e) => 
            {
                Status = ConnectionStatus.Connected;
                Ready?.Invoke(sender, null);
            };
            Client.OnJoin += (sender, e) => ActivityJoin?.Invoke(sender, e.Secret);
            Client.OnJoinRequested += (sender, e) => ActivityJoinRequest?.Invoke(sender, e.User.ToMultiRPCUser());
            Client.OnSpectate += (sender, e) => ActivitySpectate?.Invoke(sender, e.Secret);
            Client.OnError += (sender, e) =>
            {
                Status = ConnectionStatus.Connecting;
                Errored?.Invoke(this, null);
            };
            Client.Initialize();

            Status = ConnectionStatus.Connecting;

            Loading?.Invoke(this, null);
        }

        public void Stop()
        {
            IsRunning = false;

            ClearPresence();
            Client?.Deinitialize();
            Client?.Dispose();
            Status = ConnectionStatus.Disconnected;

            //TODO: See what the bool is for
            Disconnected?.Invoke(this, true);
        }

        public Result UpdatePresence(RichPresence richPresence)
        {
            if (richPresence == null)
            {
                return Result.Failed;
            }

            if (RpcPageManager.CurrentPage.RichPresence
                  .Timestamp?.SetStartOnRPCConnection ?? false && 
                RpcPageManager.CurrentPage.RichPresence
                  .Timestamp.Start == null)
            {
                RpcPageManager.CurrentPage.RichPresence
                .Timestamp.Start = DateTime.Now;
            }

            ActivePresence = richPresence;
            if (!Client?.IsDisposed ?? false) 
            {
                Client?.SetPresence(richPresence.ToDiscordRPCPresence());
            }

            return Result.Success;
        }
    }

    public static class RpcClientEx
    {
        public static User ToMultiRPCUser([NotNull] this DiscordRPC.User user) => new()
        {
            Avatar = user.Avatar,
            ID = long.Parse(user.ID.ToString()),
            Discriminator = user.Discriminator.ToString(),
            Username = user.Username
        };

        public static RichPresence ToMultiRPCPresence([NotNull] this DiscordRPC.RichPresence richPresence, string name, long id) =>
            new(name, id)
            {
                Assets = richPresence.HasAssets() ? new Core.Rpc.Assets
                {
                    LargeImage = new Image 
                    { 
                        Key = richPresence.Assets.LargeImageKey,
                        Text = richPresence.Assets.LargeImageText,
                        Uri = richPresence.Assets?.LargeImageID > 0 ?
                        new Uri($"https://cdn.discordapp.com/app-assets/{id}/{richPresence.Assets?.LargeImageID}.png") 
                        : null
                    },
                    SmallImage = new Image
                    {
                        Key = richPresence.Assets.SmallImageKey,
                        Text = richPresence.Assets.SmallImageText,
                        Uri = richPresence.Assets?.SmallImageID > 0 ?
                        new Uri($"https://cdn.discordapp.com/app-assets/{id}/{richPresence.Assets?.SmallImageID}.png")
                        : null
                    }
                } : null,
                Details = richPresence.Details,
                State = richPresence.State,
                Party = richPresence.HasParty() ? new Core.Rpc.Party
                {
                    ID = richPresence.Party.ID,
                    MaxSize = richPresence.Party.Max,
                    Size = richPresence.Party.Size
                } : null,
                Secret = richPresence.HasSecrets() ? new Secret
                {
                    Join = richPresence.Secrets.JoinSecret,
                    Spectate = richPresence.Secrets.SpectateSecret,
                    Match = richPresence.Secrets.MatchSecret                    
                } : null,
                Timestamp = richPresence.HasTimestamps() ? new Timestamp
                {
                    Start = richPresence.Timestamps.Start,
                    End = richPresence.Timestamps.End
                } : null
            };

        public static DiscordRPC.RichPresence ToDiscordRPCPresence([NotNull] this RichPresence richPresence) =>
            new()
            {
                Assets = richPresence.Assets?.LargeImage != null || richPresence.Assets?.SmallImage != null ?
                new DiscordRPC.Assets
                {
                    LargeImageKey = richPresence.Assets.LargeImage?.Key,
                    LargeImageText = richPresence.Assets.LargeImage?.Text,
                    SmallImageKey = richPresence.Assets.SmallImage?.Key,
                    SmallImageText = richPresence.Assets.SmallImage?.Text,
                } : null,
                Details = richPresence.Details,
                State = richPresence.State,
                Party = richPresence.Party?.IsValid() ?? false ? new DiscordRPC.Party
                {
                    ID = richPresence.Party.ID,
                    Size = richPresence.Party.Size,
                    Max = richPresence.Party.MaxSize
                } : null,
                Secrets = richPresence.Secret?.IsValid() ?? false ? new Secrets
                {
                    JoinSecret = richPresence.Secret.Join,
                    SpectateSecret = richPresence.Secret.Spectate,
                    MatchSecret = richPresence.Secret.Match
                } : null,
                Timestamps = richPresence.Timestamp?.IsValid() ?? false ? new Timestamps
                {
                    End = richPresence.Timestamp.End,
                    Start = richPresence.Timestamp.Start
                } : null
            };
    }
}
