using DiscordRPC;
using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace MultiRPC
{
    public static class RPC
    {
        public static Config Config = new Config();
        public static string ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC.json";
        public static HttpClient HttpClient = new HttpClient();
        public static Logger Log = new Logger();
        //public string Type;
        private static System.Timers.Timer UpdateTimer = new System.Timers.Timer(new TimeSpan(0, 1, 0).TotalMilliseconds);
        public static DiscordRpcClient Client = null;
        private static System.Timers.Timer ClientTimer;
        public static DiscordRPC.RichPresence Presence = new RichPresence();

        public static void Start(ulong id)
        {
            //UpdateTimer.Elapsed += UpdateRPC;
            Log.App("Starting MultiRPC");
            //Create a new client
            Client = new DiscordRpcClient(id.ToString());

            Client.OnClose += Client_OnClose;
            Client.OnConnectionEstablished += Client_OnConnectionEstablished;
            Client.OnConnectionFailed += Client_OnConnectionFailed;
            Client.OnError += Client_OnError;
            Client.OnPresenceUpdate += Client_OnPresenceUpdate;
            Client.OnReady += Client_OnReady;
            Client.OnSubscribe += Client_OnSubscribe;
            Client.OnUnsubscribe += Client_OnUnsubscribe;

            //Create a timer that will regularly call invoke
            ClientTimer = new System.Timers.Timer(150);
            ClientTimer.Elapsed += (sender, evt) => { Client.Invoke(); };
            ClientTimer.Start();

            //Connect
            Client.Initialize();

            //Send a presence. Do this as many times as you want
            Client.SetPresence(Presence);
        }

        public static void SetPresence(MainWindow window)
        {
            Presence.Details = window.Text_CustomText1.Text;
            Presence.State = window.Text_CustomText2.Text;
            Presence.Assets = new DiscordRPC.Assets
            {
                LargeImageKey = window.Text_CustomLargeKey.Text,
                LargeImageText = window.Text_CustomLargeText.Text,
                SmallImageKey = window.Text_CustomSmallKey.Text,
                SmallImageText = window.Text_CustomSmallText.Text
            };
        }

        public static void Update()
        {
            Client.SetPresence(Presence);
        }

        private static void Client_OnUnsubscribe(object sender, DiscordRPC.Message.UnsubscribeMessage args)
        {
            Log.Discord($"Unsub {args.Event}");
        }

        private static void Client_OnSubscribe(object sender, DiscordRPC.Message.SubscribeMessage args)
        {
            Log.Discord($"Sub {args.Event}");
        }

        private static void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            Log.Discord($"RPC ready, found user {args.User.Username}#{args.User.Discriminator} | {args.Version}");
        }

        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            MainWindow.SetLiveView(args);
            Log.Discord($"Updated presence");
        }

        private static void Client_OnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            Log.Discord($"Error ({args.Code}) {args.Message}");
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            Log.Discord($"Failed connection {args.FailedPipe} {args.Type}");
        }

        private static void Client_OnConnectionEstablished(object sender, DiscordRPC.Message.ConnectionEstablishedMessage args)
        {
            Log.Discord($"Connected {args.ConnectedPipe} {args.Type}");
        }

        private static void Client_OnClose(object sender, DiscordRPC.Message.CloseMessage args)
        {
            Log.Discord($"Closed ({args.Code}) {args.Reason}");
        }

        public static void Shutdown()
        {
            Log.App("Shutting down RPC");
            ClientTimer.Dispose();
            Client.Dispose();
        }

        public static bool CheckPort()
        {
            bool isAvailable = true;
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Address.ToString() == "127.0.0.1" && endpoint.Port > 6462 && endpoint.Port < 6473)
                {
                    Log.App(endpoint.Port.ToString());
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}

