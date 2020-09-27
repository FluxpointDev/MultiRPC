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

        void Start(long applicationID);

        void Stop();

        void RegisterCommand(string command);

        void RegisterSteam(int gameID);

        //Make custom result for this due to rate limiting
        Result UpdatePresence(RichPresence richPresence);

        Result ClearPresence();

        Result SendRequestReply(long userID, JoinRequestReply reply);

        Result SendInvite(long userID, ActionType actionType, string message);

        Result AcceptInvite(long userID);

        event EventHandler<string> OnActivityJoin;

        event EventHandler<string> OnActivitySpectate;

        event EventHandler<User> OnActivityJoinRequest;

        event EventHandler<Invite> OnActivityInvite;

        /// <summary>
        /// Fires when we start loading in everything to get <see cref="Ready"/>
        /// </summary>
        event EventHandler Loading;

        event EventHandler Ready;

        event EventHandler Errored;

        event EventHandler<bool> Disconnected;
    }
}
