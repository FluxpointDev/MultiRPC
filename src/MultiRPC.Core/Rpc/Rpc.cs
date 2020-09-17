/*using System;
using System.IO;
using DiscordRPC;
using Serilog.Events;
using DiscordRPC.Message;
using JetBrains.Annotations;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Notification;

namespace MultiRPC.Core.Rpc
{
    //Make this a interface for easily changing it without breaking 10000 other stuff
    /// <summary>
    /// All logic for doing <see cref="DiscordRPC.RichPresence"/>
    /// </summary>
    public static class Rpc
    {
        /// <summary>
        /// The time that the Client started sending <see cref="DiscordRPC.RichPresence"/>'s
        /// </summary>
        public static DateTime RpcStartTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// Client to be used for Rpc useage
        /// </summary>
        [CanBeNull]
        public static DiscordRpcClient Client { get; private set; }

        //Allows us to track when the client is *really* connected
        private static bool hasConnection = false;
        /// <summary>
        /// If <see cref="Client"/> has an connection
        /// </summary>
        [NotNull]
        public static bool HasConnection => (Client?.IsInitialized ?? false) && hasConnection;

        /// <summary>
        /// The application ID to use for the RPC connection
        /// </summary>
        public static ulong ApplicationID { get; set; } = 0;

        private static RichPresence richPresence = new RichPresence();
        /// <summary>
        /// The <see cref="DiscordRPC.RichPresence"/> that will be used with the <see cref="Client"/>
        /// </summary>
        [NotNull]
        public static RichPresence RichPresence 
        {
            get => richPresence;
            set 
            {
                richPresence = value;
                if (richPresence.CanBeSet())
                {
                    Client?.SetPresence(value);
                }
            }
        } 

        /// <summary>
        /// Fires when the connection gets closed successfuly
        /// </summary>
        [CanBeNull]
        public static event EventHandler ConnectionClosed;

        /// <summary>
        /// Fires when the connection gets opened and is ready for us to use
        /// </summary>
        [CanBeNull]
        public static event EventHandler<ReadyMessage> ConnectionOpened;

        /// <summary>
        /// Fires when the connection is being attempted
        /// </summary>
        [CanBeNull]
        public static event EventHandler ConnectionStarted;

        /// <summary>
        /// Fires when the connection is lost but isn't closed
        /// </summary>
        [CanBeNull]
        public static event EventHandler<ConnectionFailedMessage> ConnectionLost;

        /// <summary>
        /// Fires when the Rpc has been updated
        /// </summary>
        [CanBeNull]
        public static event EventHandler<PresenceMessage> RpcUpdated;

        /// <summary>
        /// Get if this presence is ban worthy (Will always return false when <see cref="Settings.InviteWarn"/> is false)
        /// </summary>
        /// <param name="text"></param>
        public static bool CheckPresence(RichPresence presence)
        {
            //TODO: Readd
            return false;
            //return !Settings.Current.InviteWarn && 
            //  (!string.IsNullOrWhiteSpace(presence.Details) && presence.Details.ToLower().Contains("discord.gg")) || 
            //  (!string.IsNullOrWhiteSpace(presence.State) && presence.State.ToLower().Contains("discord.gg")) ||
            //  (!string.IsNullOrWhiteSpace(presence.Assets?.LargeImageText) && presence.Assets.LargeImageText.ToLower().Contains("discord.gg")) ||
            //  (!string.IsNullOrWhiteSpace(presence.Assets?.SmallImageText) && presence.Assets.SmallImageText.ToLower().Contains("discord.gg"));
        }

        /// <summary>
        /// Sets up and starts Rpc
        /// </summary>
        /// <param name="applicationID">Application ID to be used</param>
        public static void StartRpc([CanBeNull] ulong? applicationID = null)
        {
            if (CheckPresence(RichPresence))
            {
                NotificationToast.Create("AdvertisingWarning", LogEventLevel.Warning, dismissive: false, notificationToastButtons: new NotificationToastButton[] 
                {
                    NotificationToastButton.Create("Ok", toast =>
                    {
                        toast.Dismissive = true;
                        //Settings.Current.InviteWarn = true; //TODO: Readd
                        StartRpc(applicationID);
                    }),
                    NotificationToastButton.Create("Cancel", toast => toast.DismissToast())
                });
                ConnectionClosed?.Invoke(null, EventArgs.Empty);
                return;
            }

            var foundClient = false;
            var pipeCount = -1;
            var discordClientName = "";

            //Get the client to connect to
            var pipes = new string[0];//Settings.Current.ClientToUse != Enums.DiscordClient.Any ?
                //Directory.GetFiles(@"\\.\pipe\") :
                //null; //TODO: Readd
            for (var i = 0; i < pipes?.Length; i++)
            {
                var pipe = Path.GetFileName(pipes[i]);

                /*switch (pipe)
                {
                    case "discord-sock"
                    when Settings.Current.ClientToUse == Enums.DiscordClient.Discord:
                    {
                        discordClientName = "Discord";
                        foundClient = true;
                    }
                    break;

                    case "discordptb-sock"
                    when Settings.Current.ClientToUse == Enums.DiscordClient.DiscordPTB:
                    {
                        discordClientName = "Discord PTB";
                        foundClient = true;
                    }
                    break;

                    case "discordcanary-sock"
                    when Settings.Current.ClientToUse == Enums.DiscordClient.DiscordCanary:
                    {
                        discordClientName = "Discord Canary";
                        foundClient = true;
                    }
                    break;
                }

                if (pipe.StartsWith("discord"))
                {
                    pipeCount++;
                }

                if (foundClient)
                {
                    break;
                }
            }

            if (!foundClient)
            {
                NotificationCenter.Logger.Warning(LanguagePicker.GetLineFromLanguageFile("DidNotFindDiscordClient"));
            }
            else
            {
                NotificationCenter.Logger.Information($"{LanguagePicker.GetLineFromLanguageFile("FoundDiscordClient")}: {discordClientName} ({LanguagePicker.GetLineFromLanguageFile("Pipe")} {pipeCount})");
            }

            Client = new DiscordRpcClient(applicationID?.ToString() ?? ApplicationID.ToString(), pipeCount)
            {
                SkipIdenticalPresence = false,
                Logger = RpcLogger.Current
            };

            if (RichPresence.Timestamps?.Start == RpcStartTime)
            {
                RichPresence.Timestamps.Start = DateTime.Now;
                RpcStartTime = RichPresence.Timestamps.Start.Value;
            }
            else
            {
                RpcStartTime = DateTime.Now;
            }
            Client.SetPresence(RichPresence);
            Client.OnReady += (sender, args) =>
            {
                hasConnection = true;
                ConnectionOpened?.Invoke(sender, args);
            };
            Client.OnConnectionFailed += (sender, args) =>
            {
                hasConnection = false;
                ConnectionLost?.Invoke(sender, args);
            };
            Client.OnPresenceUpdate += (sender, msg) =>
            {
                RpcUpdated?.Invoke(sender, msg);
            };

            Client.Initialize();
            ConnectionStarted?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Stops Rpc and fires out <see cref="ConnectionClosed"/>
        /// </summary>
        public static void StopRpc()
        {
            if (Client?.IsInitialized ?? false) 
            {
                Client?.ClearPresence();
                Client?.Deinitialize();
                ConnectionClosed?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}*/