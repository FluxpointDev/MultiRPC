using DiscordRPC.Events;
using DiscordRPC.Exceptions;
using DiscordRPC.IO;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using DiscordRPC.Registry;
using DiscordRPC.RPC;
using DiscordRPC.RPC.Commands;
using System;

namespace DiscordRPC
{
    public class DiscordRpcClient : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets a value indicating if the RPC Client has registered a URI scheme. If this is false, Join / Spectate events will fail.
        /// </summary>
        public bool HasRegisteredUriScheme { get; private set; }

        /// <summary>
        /// Gets the Application ID of the RPC Client.
        /// </summary>
        public string ApplicationID { get; private set; }

        /// <summary>
        /// Gets the Steam ID of the RPC Client. This value can be null if none was supplied.
        /// </summary>
        public string SteamID { get; private set; }

        /// <summary>
        /// Gets the ID of the process used to run the RPC Client. Discord tracks this process ID and waits for its termination.
        /// </summary>
        public int ProcessID { get; private set; }

        /// <summary>
        /// The dispose state of the client object.
        /// </summary>
        public bool Disposed { get { return _disposed; } }
        private bool _disposed = false;

        /// <summary>
        /// The logger used this client and its associated components. <see cref="ILogger"/> are not called safely and can come from any thread. It is upto the <see cref="ILogger"/> to account for this and apply appropriate thread safe methods.
        /// </summary>
        public ILogger Logger
        {
            get { return _logger; }
            set
            {
                this._logger = value;
                if (connection != null) connection.Logger = value;
            }
        }
        private ILogger _logger = new NullLogger();
        #endregion

        /// <summary>
        /// The pipe the discord client is on, ranging from 0 to 9. Use -1 to scan through all pipes.
        /// <para>This property can be used for testing multiple clients. For example, if a Discord Client was on pipe 0, the Discord Canary is most likely on pipe 1.</para>
        /// </summary>
        public int TargetPipe { get { return _pipe; } }
        private int _pipe = -1;
        private RpcConnection connection;

        /// <summary>
        /// The current presence that the client has. Gets set with <see cref="SetPresence(RichPresence)"/> and updated on <see cref="OnPresenceUpdate"/>.
        /// </summary>
        public RichPresence CurrentPresence { get { return _presence; } }
        private RichPresence _presence;

        /// <summary>
        /// The current discord user. This is updated with the ready event and will be null until the event is fired from the connection.
        /// </summary>
        public User CurrentUser { get { return _user; } }
        private User _user;

        /// <summary>
        /// The current configuration the connection is using. Only becomes available after a ready event.
        /// </summary>
        public Configuration Configuration { get { return _configuration; } }
        private Configuration _configuration;

        /// <summary>
        /// Represents if the client has been <see cref="Initialize"/>
        /// </summary>
        public bool IsInitialized { get { return _initialized; } }
        private bool _initialized;

        /// <summary>
        /// Forces the connection to shutdown gracefully instead of just aborting the connection.
        /// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
        /// </summary>
        public bool ShutdownOnly { get { return _shutdownOnly; } set { _shutdownOnly = value; if (connection != null) connection.ShutdownOnly = value; } }
        private bool _shutdownOnly = true;

        #region Events

        /// <summary>
        /// Called when the discord client is ready to send and receive messages.
        /// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
        /// </summary>
        public event OnReadyEvent OnReady;

        /// <summary>
        /// Called when connection to the Discord Client is lost. The connection will remain close and unready to accept messages until the Ready event is called again.
        /// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
        /// </summary>
        public event OnCloseEvent OnClose;

        /// <summary>
        /// Called when a error has occured during the transmission of a message. For example, if a bad Rich Presence payload is sent, this event will be called explaining what went wrong.
        /// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
        /// </summary>
        public event OnErrorEvent OnError;

        /// <summary>
        /// Called when the Discord Client has updated the presence.
        /// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
        /// </summary>
        public event OnPresenceUpdateEvent OnPresenceUpdate;

        /// <summary>
        /// The connection to the discord client was succesfull. This is called before <see cref="MessageType.Ready"/>.
        /// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
        /// </summary>
        public event OnConnectionEstablishedEvent OnConnectionEstablished;

        /// <summary>
        /// Failed to establish any connection with discord. Discord is potentially not running?
        /// </summary>
        public event OnConnectionFailedEvent OnConnectionFailed;
        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new Discord RPC Client without using any uri scheme. This will disable the Join / Spectate functionality.
        /// </summary>
        /// <param name="applicationID"></param>
        public DiscordRpcClient(string applicationID) : this(applicationID, -1) { }

        /// <summary>
        /// Creates a new Discord RPC Client without using any uri scheme. This will disable the Join / Spectate functionality.
        /// </summary>
        /// <param name="applicationID"></param>	
        /// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
        public DiscordRpcClient(string applicationID, int pipe) : this(applicationID, null, false, pipe) { }


        /// <summary>
        /// Creates a new Discord RPC Client using the default uri scheme.
        /// </summary>
        /// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
        /// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
        public DiscordRpcClient(string applicationID, bool registerUriScheme) : this(applicationID, registerUriScheme, -1) { }

        /// <summary>
        /// Creates a new Discord RPC Client using the default uri scheme.
        /// </summary>
        /// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
        /// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
        /// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
        public DiscordRpcClient(string applicationID, bool registerUriScheme, int pipe) : this(applicationID, null, registerUriScheme, pipe) { }

        /// <summary>
        /// Creates a new Discord RPC Client using the steam uri scheme.
        /// </summary>
        /// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
        /// <param name="steamID">The steam ID of the app. This is used to launch Join / Spectate through steam URI scheme instead of manual launching</param>
        /// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
        public DiscordRpcClient(string applicationID, string steamID, bool registerUriScheme) : this(applicationID, steamID, registerUriScheme, -1) { }

        /// <summary>
        /// Creates a new Discord RPC Client using the steam uri scheme.
        /// </summary>
        /// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
        /// <param name="steamID">The steam ID of the app. This is used to launch Join / Spectate through steam URI scheme instead of manual launching</param>
        /// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
        /// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
        public DiscordRpcClient(string applicationID, string steamID, bool registerUriScheme, int pipe) : this(applicationID, steamID, registerUriScheme, pipe, new ManagedNamedPipeClient()) { }

        /// <summary>
        /// Creates a new Discord RPC Client using the steam uri scheme.
        /// </summary>
        /// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
        /// <param name="steamID">The steam ID of the app. This is used to launch Join / Spectate through steam URI scheme instead of manual launching</param>
        /// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
        /// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
        /// <param name="client">The pipe client to use and communicate to discord through</param>
        public DiscordRpcClient(string applicationID, string steamID, bool registerUriScheme, int pipe, INamedPipeClient client)
        {

            //Store our values
            ApplicationID = applicationID;
            SteamID = steamID;
            HasRegisteredUriScheme = registerUriScheme;
            ProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
            _pipe = pipe;

            //If we are to register the URI scheme, do so.
            //The UriScheme.RegisterUriScheme function takes steamID as a optional parameter, its null by default.
            //   this means it will handle a null steamID for us :)
            if (registerUriScheme)
                UriScheme.RegisterUriScheme(applicationID, steamID);

            //Create the RPC client
            connection = new RpcConnection(ApplicationID, ProcessID, TargetPipe, client) { ShutdownOnly = _shutdownOnly };
            connection.Logger = this._logger;
        }

        #endregion

        #region Message Handling
        /// <summary>
        /// Dequeues all the messages from Discord and invokes appropriate methods. This will process the message and update the internal state before invoking the events. Returns the messages that were invoked and in the order they were invoked.
        /// </summary>
        /// <returns>Returns the messages that were invoked and in the order they were invoked.</returns>
        public IMessage[] Invoke()
        {
            //Dequeue all the messages and process them
            IMessage[] messages = connection.DequeueMessages();
            for (int i = 0; i < messages.Length; i++)
            {
                //Do a bit of pre-processing
                IMessage message = messages[i];
                HandleMessage(message);

                //Invoke the appropriate methods
                switch (message.Type)
                {
                    case MessageType.Ready:
                        if (OnReady != null) OnReady.Invoke(this, message as ReadyMessage);
                        break;

                    case MessageType.Close:
                        if (OnClose != null) OnClose.Invoke(this, message as CloseMessage);
                        break;

                    case MessageType.Error:
                        if (OnError != null) OnError.Invoke(this, message as ErrorMessage);
                        break;

                    case MessageType.PresenceUpdate:
                        if (OnPresenceUpdate != null) OnPresenceUpdate.Invoke(this, message as PresenceMessage);
                        break;

                    case MessageType.ConnectionEstablished:
                        if (OnConnectionEstablished != null) OnConnectionEstablished.Invoke(this, message as ConnectionEstablishedMessage);
                        break;

                    case MessageType.ConnectionFailed:
                        if (OnConnectionFailed != null) OnConnectionFailed.Invoke(this, message as ConnectionFailedMessage);
                        break;

                    default:
                        //This in theory can never happen, but its a good idea as a reminder to update this part of the library if any new messages are implemented.
                        Logger.Error("Message was queued with no appropriate handle! {0}", message.Type);
                        break;
                }
            }

            //Finally, return the messages
            return messages;
        }

        /// <summary>
        /// Gets a single message from the queue. This may return null if none are availble. This will process the message and update internal state before handing it over.
        /// </summary>
        /// <returns></returns>
        public IMessage Dequeue()
        {
            if (Disposed)
                throw new ObjectDisposedException("Discord IPC Client");

            //Dequeue the message and do some preprocessing
            IMessage message = connection.DequeueMessage();
            HandleMessage(message);

            //return the message
            return message;
        }

        /// <summary>
        /// Dequeues all messages from the Discord queue. This will be a empty array of size 0 if none are availble. This will process the messages and update internal state before handing it over.
        /// </summary>
        /// <returns></returns>
        public IMessage[] DequeueAll()
        {
            if (Disposed)
                throw new ObjectDisposedException("Discord IPC Client");

            //Dequeue all the messages and process them
            IMessage[] messages = connection.DequeueMessages();
            for (int i = 0; i < messages.Length; i++) HandleMessage(messages[i]);

            //Return it
            return messages;
        }

        private void HandleMessage(IMessage message)
        {
            if (message == null) return;
            switch (message.Type)
            {
                //We got a update, so we will update our current presence
                case MessageType.PresenceUpdate:
                    PresenceMessage pm = message as PresenceMessage;
                    if (pm != null)
                    {
                        //We need to merge these presences together
                        if (_presence == null)
                        {
                            _presence = pm.Presence;
                        }
                        else if (pm.Presence == null)
                        {
                            _presence = null;
                        }
                        else
                        {
                            _presence.Merge(pm.Presence);
                        }

                        //Update the message
                        pm.Presence = _presence;
                    }

                    break;

                //Update our configuration
                case MessageType.Ready:
                    ReadyMessage rm = message as ReadyMessage;
                    if (rm != null)
                    {
                        _configuration = rm.Configuration;
                        _user = rm.User;

                        //Resend our presence and subscription
                        SynchronizeState();
                    }
                    break;

                //We got a message we dont know what to do with.
                default:
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Sets the Rich Presences
        /// </summary>
        /// <param name="presence">The rich presence to send to discord</param>
        public void SetPresence(RichPresence presence)
        {
            if (Disposed)
                throw new ObjectDisposedException("Discord IPC Client");

            if (connection == null)
                throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

            //Update our internal store of the presence
            _presence = presence;
            if (!_presence)
            {
                //Clear the presence
                connection.EnqueueCommand(new PresenceCommand() { PID = this.ProcessID, Presence = null });
            }
            else
            {
                //Send the presence
                connection.EnqueueCommand(new PresenceCommand() { PID = this.ProcessID, Presence = presence.Clone() });
            }
        }

        #region Updates
        /// <summary>
        /// Updates only the <see cref="RichPresence.Details"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Returns the newly edited Rich Presence.
        /// </summary>
        /// <param name="details">The details of the Rich Presence</param>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateDetails(string details)
        {
            if (_presence == null) _presence = new RichPresence();
            _presence.Details = details;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }
        /// <summary>
        /// Updates only the <see cref="RichPresence.State"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Returns the newly edited Rich Presence.
        /// </summary>
        /// <param name="state">The state of the Rich Presence</param>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateState(string state)
        {
            if (_presence == null) _presence = new RichPresence();
            _presence.State = state;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }

        /// <summary>
        /// Updates the large <see cref="Assets"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Both <paramref name="key"/> and <paramref name="tooltip"/> are optional and will be ignored it null.
        /// </summary>
        /// <param name="key">Optional: The new key to set the asset too</param>
        /// <param name="tooltip">Optional: The new tooltip to display on the asset</param>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateLargeAsset(string key = null, string tooltip = null)
        {
            if (_presence == null) _presence = new RichPresence();
            if (_presence.Assets == null) _presence.Assets = new Assets();
            _presence.Assets.LargeImageKey = key ?? _presence.Assets.LargeImageKey;
            _presence.Assets.LargeImageText = tooltip ?? _presence.Assets.LargeImageText;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }

        /// <summary>
        /// Updates the small <see cref="Assets"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Both <paramref name="key"/> and <paramref name="tooltip"/> are optional and will be ignored it null.
        /// </summary>
        /// <param name="key">Optional: The new key to set the asset too</param>
        /// <param name="tooltip">Optional: The new tooltip to display on the asset</param>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateSmallAsset(string key = null, string tooltip = null)
        {
            if (_presence == null) _presence = new RichPresence();
            if (_presence.Assets == null) _presence.Assets = new Assets();
            _presence.Assets.SmallImageKey = key ?? _presence.Assets.SmallImageKey;
            _presence.Assets.SmallImageText = tooltip ?? _presence.Assets.SmallImageText;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }

        /// <summary>
        /// Sets the start time of the <see cref="CurrentPresence"/> to now and sends the updated presence to Discord.
        /// </summary>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateStartTime() { try { return UpdateStartTime(DateTime.UtcNow); } catch (Exception) { throw; } }

        /// <summary>
        /// Sets the start time of the <see cref="CurrentPresence"/> and sends the updated presence to Discord.
        /// </summary>
        /// <param name="time">The new time for the start</param>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateStartTime(DateTime time)
        {
            if (_presence == null) _presence = new RichPresence();
            if (_presence.Timestamps == null) _presence.Timestamps = new Timestamps();
            _presence.Timestamps.Start = time;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }

        /// <summary>
        /// Sets the end time of the <see cref="CurrentPresence"/> to now and sends the updated presence to Discord.
        /// </summary>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateEndTime() { try { return UpdateEndTime(DateTime.UtcNow); } catch (Exception) { throw; } }

        /// <summary>
        /// Sets the end time of the <see cref="CurrentPresence"/> and sends the updated presence to Discord.
        /// </summary>
        /// <param name="time">The new time for the end</param>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateEndTime(DateTime time)
        {
            if (_presence == null) _presence = new RichPresence();
            if (_presence.Timestamps == null) _presence.Timestamps = new Timestamps();
            _presence.Timestamps.End = time;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }

        /// <summary>
        /// Sets the start and end time of <see cref="CurrentPresence"/> to null and sends it to Discord.
        /// </summary>
        /// <returns>Updated Rich Presence</returns>
        public RichPresence UpdateClearTime()
        {
            if (_presence == null) return null;
            _presence.Timestamps = null;
            try { SetPresence(_presence); } catch (Exception) { throw; }
            return _presence;
        }
        #endregion

        /// <summary>
        /// Clears the Rich Presence. Use this just before disposal to prevent ghosting.
        /// </summary>
        public void ClearPresence()
        {
            if (Disposed)
                throw new ObjectDisposedException("Discord IPC Client");

            if (connection == null)
                throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

            //Just a wrapper function for sending null
            SetPresence(null);
        }
        

        /// <summary>
        /// Resends the current presence and subscription. This is used when Ready is called to keep the current state within discord.
        /// </summary>
        public void SynchronizeState()
        {
            //Set the presence 
            SetPresence(_presence);
        }

        /// <summary>
        /// Attempts to initalize a connection to the Discord IPC.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            if (Disposed)
                throw new ObjectDisposedException("Discord IPC Client");

            if (connection == null)
                throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

            return _initialized = connection.AttemptConnection();
        }

        /// <summary>
        /// Terminates the connection to Discord and disposes of the object.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;

            connection.Close();
            _disposed = true;
        }

    }
}
