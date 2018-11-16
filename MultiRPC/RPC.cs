using DiscordRPC;
using MultiRPC.GUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Windows;

namespace MultiRPC
{
    public static class RPC
    {
        public static Config Config = new Config();
        public static string ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC.json";
        public static HttpClient HttpClient = new HttpClient();
        public static Logger Log = new Logger("Rpc");
        private readonly static System.Timers.Timer UpdateTimer = new System.Timers.Timer(new TimeSpan(0, 1, 0).TotalMilliseconds);
        public static DiscordRpcClient Client = null;
        private static System.Timers.Timer ClientTimer;
        public static RichPresence Presence = new RichPresence();
        /// <summary>
        /// Has rpc failed
        /// </summary>
        public static bool Failed = false;
        /// <summary>
        /// Is afk rpc status on
        /// </summary>
        public static bool AFK = false;
        /// <summary>
        /// Type of rpc to resume when afk toggled off
        /// </summary>
        public static string AFKResume = "";
        public static string Type = "default";

        //public static bool LoadingSettings = true;
        public static void CheckField(string text)
        {
            if (!Config.InviteWarn && text.ToLower().Contains("discord.gg/"))
            {
                Config.InviteWarn = true;
                Config.Save();
                MessageBox.Show("Advertising in rpc could result in you being kicked or banned from servers!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void Start(ulong id)
        {
            FirstUpdate = false;
            Failed = true;
            //UpdateTimer.Elapsed += UpdateRPC;
            Log.Write("Starting MultiRPC");
            //Create a new client
            Client = new DiscordRpcClient(id.ToString(),false, int.Parse(MainWindow.WD.ItemsPipe.Text) - 2);
            Client.OnClose += Client_OnClose;
            Client.OnConnectionEstablished += Client_OnConnectionEstablished;
            Client.OnConnectionFailed += Client_OnConnectionFailed;
            Client.OnError += Client_OnError;
            Client.OnPresenceUpdate += Client_OnPresenceUpdate;
            Client.OnReady += Client_OnReady;
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
            Presence.Details = window.TextCustomText1.Text;
            Presence.State = window.TextCustomText2.Text;
            Presence.Assets = new Assets
            {
                LargeImageKey = window.TextCustomLargeKey.Text,
                LargeImageText = window.TextCustomLargeText.Text,
                SmallImageKey = window.TextCustomSmallKey.Text,
                SmallImageText = window.TextCustomSmallText.Text
            };
        }

        public static void SetPresence(string text1, string text2, string largeKey, string largeText, string smallKey, string smallText)
        {
            Presence.Details = text1;
            Presence.State = text2;
            Presence.Assets = new Assets
            {
                LargeImageKey = largeKey,
                LargeImageText = largeText,
                SmallImageKey = smallKey,
                SmallImageText = smallText
            };
        }

        public static void Update()
        {
            Client.SetPresence(Presence);
        }

        private static void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            Log.Write($"Ready, hi {args.User.Username}#{args.User.Discriminator} 👋");
            MainWindow.WD.TextUser.Dispatcher.BeginInvoke((Action)delegate ()
            {
                MainWindow.WD.TextUser.Content = args.User.ToString();
                MainWindow.DisableElements(true);
            });
            
            MainWindow.WD.TextStatus.Dispatcher.BeginInvoke((Action)delegate ()
            {
                MainWindow.DisableElements(true);
            });
        }

        public static bool FirstUpdate = false;
        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            if (FirstUpdate)
            {
                Log.Write($"Updated presence for {args.Name}");
                MainWindow.SetLiveView(args);
            }
            FirstUpdate = true;
        }

        private static void Client_OnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            Log.Error($"({args.Code}) {args.Message}");
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            if (!Failed)
            {
                Failed = true;
                Log.Error($"Discord client invalid, {args.Type}");
                MainWindow.SetLiveView(ViewType.Error, "Attempting to reconnect");
            }
        }

        private static void Client_OnConnectionEstablished(object sender, DiscordRPC.Message.ConnectionEstablishedMessage args)
        {
            Failed = false;
            Log.Write($"Connected");
        }

        private static void Client_OnClose(object sender, DiscordRPC.Message.CloseMessage args)
        {
            Log.Write($"Closed ({args.Code}) {args.Reason}");
        }

        public static void Shutdown()
        {
            Log.Write("Shutting down");
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
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}

