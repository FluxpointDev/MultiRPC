using MultiRPC.Core.Rpc;
using System;
using DiscordRPC;
using RichPresence = MultiRPC.Core.Rpc.RichPresence;
using User = MultiRPC.Core.Rpc.User;

namespace MultiRPC.UWP
{
    class RpcClient : IRpcClient
    {
        private DiscordRpcClient Client;

        public bool IsRunning { get; private set; }

        public event EventHandler<string> OnActivityJoin;
        public event EventHandler<string> OnActivitySpectate;
        public event EventHandler<User> OnActivityJoinRequest;
        public event EventHandler<Invite> OnActivityInvite; //TODO: Find out what I need to do to hook this up
        public event EventHandler Ready;
        public event EventHandler Errored;
        public event EventHandler<bool> Disconnected;
        public event EventHandler Loading;

        public Result AcceptInvite(long userID)
        {
            throw new NotImplementedException();
        }

        public Result ClearPresence()
        {
            throw new NotImplementedException();
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

        public void Start(long applicationID)
        {
            if (IsRunning)
            {
                Stop();
            }

            Client = new DiscordRpcClient(applicationID.ToString()) //TODO: Add custom pipe support
            {
                SkipIdenticalPresence = false,
                //Logger = RpcLogger.Current //TODO: Add Logger
            };
            Client.ShutdownOnly = true;

            UpdatePresence(RpcPageManager.CurrentPage.RichPresence);
            Client.OnReady += (sender, e) => Ready?.Invoke(sender, null);
            Client.OnJoin += (sender, e) => OnActivityJoin?.Invoke(sender, e.Secret);
            Client.OnJoinRequested += (sender, e) => OnActivityJoinRequest?.Invoke(sender, e.User.ToMultiRPCUser());
            Client.OnSpectate += (sender, e) => OnActivitySpectate?.Invoke(sender, e.Secret);
            Client.OnError += (sender, e) => Errored?.Invoke(this, null);

            Loading?.Invoke(this, null);
        }

        public void Stop()
        {
            ClearPresence();
            Client?.Dispose();
            IsRunning = false;

            //TODO: See what the bool is for
            Disconnected?.Invoke(this, true);
        }

        public Result UpdatePresence(RichPresence richPresence)
        {
            if (richPresence == null)
            {
                return Result.Failed;
            }

            Client?.SetPresence(new DiscordRPC.RichPresence
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
                Party = richPresence.Party != null ? new DiscordRPC.Party
                {
                    ID = richPresence.Party.ID,
                    Size = richPresence.Party.Size,
                    Max = richPresence.Party.MaxSize
                } : null,
                Secrets = richPresence.Secret != null ? new Secrets
                {
                    JoinSecret = richPresence.Secret.Join,
                    SpectateSecret = richPresence.Secret.Spectate,
                    MatchSecret = richPresence.Secret.Match
                } : null,
                Timestamps = richPresence.Timestamp != null ? new Timestamps
                {
                    End = richPresence.Timestamp.End,
                    Start = richPresence.Timestamp.Start
                } : null
            });

            return Result.Success;
        }
    }

    public static class RpcClientEx
    {
        public static User ToMultiRPCUser(this DiscordRPC.User user) => new User()
        {
            Avatar = user.Avatar,
            ID = long.Parse(user.ID.ToString()),
            Discriminator = user.Discriminator.ToString(),
            Username = user.Username
        };
    }
}
