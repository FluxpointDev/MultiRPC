using System;
using System.Collections.Generic;
using System.Text;

namespace MultiRPC.Core.Rpc
{
    public interface IRpcClient : IRequired
    {
        /// <summary>
        /// Gets if this RpcClient is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// What the status of the running client is
        /// </summary>
        ConnectionStatus Status { get; }

        /// <summary>
        /// What the rich (at least) should be
        /// </summary>
        RichPresence ActivePresence { get; }

        void Start(long? applicationID);

        void Stop();

        void RegisterCommand(string command);

        void RegisterSteam(int gameID);

        //Make custom result for this due to rate limiting
        Result UpdatePresence(RichPresence richPresence);

        Result ClearPresence();

        Result SendRequestReply(long userID, JoinRequestReply reply);

        Result SendInvite(long userID, ActionType actionType, string message);

        Result AcceptInvite(long userID);

        event EventHandler<string> ActivityJoin;

        event EventHandler<string> ActivitySpectate;

        event EventHandler<User> ActivityJoinRequest;

        event EventHandler<Invite> ActivityInvite;

        /// <summary>
        /// Fires when we start loading in everything to get <see cref="Ready"/>
        /// </summary>
        event EventHandler Loading;

        event EventHandler<RichPresence> PresenceUpdated;

        event EventHandler Ready;

        event EventHandler Errored;

        event EventHandler<bool> Disconnected;
    }
}
