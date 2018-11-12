using DiscordRPC;
using MultiRPC.GUI;
using System;
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
        public static Logger Log = new Logger();
        private static System.Timers.Timer UpdateTimer = new System.Timers.Timer(new TimeSpan(0, 1, 0).TotalMilliseconds);
        public static DiscordRpcClient Client = null;
        private static System.Timers.Timer ClientTimer;
        public static DiscordRPC.RichPresence Presence = new RichPresence();
        /// <summary>
        /// Number of failed connection attempts
        /// </summary>
        public static int Fails = 0;
        /// <summary>
        /// Is afk rpc status on
        /// </summary>
        public static bool AFK = false;
        /// <summary>
        /// Type of rpc to resume when afk toggled off
        /// </summary>
        public static string AFKResume = "";
        public static string Type = "default";

        public static bool LoadingSettings = true;
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
            Fails = 0;
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

            //Create a timer that will regularly call invoke
            ClientTimer = new System.Timers.Timer(150);
            ClientTimer.Elapsed += (sender, evt) => { Client.Invoke(); };
            ClientTimer.Start();

            //Connect
            Client.Initialize();
            //Send a presence. Do this as many times as you want
            Client.SetPresence(Presence);
        }

        public static void SetPresence(MainWindow2 window)
        {
            Presence.Details = window.Text_CustomText1.Text;
            Presence.State = window.Text_CustomText2.Text;
            Presence.Assets = new Assets
            {
                LargeImageKey = window.Text_CustomLargeKey.Text,
                LargeImageText = window.Text_CustomLargeText.Text,
                SmallImageKey = window.Text_CustomSmallKey.Text,
                SmallImageText = window.Text_CustomSmallText.Text
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
            MainWindow2.SetRPCUser($"{args.User}");
            Log.Discord($"RPC ready, found user {args.User.Username}#{args.User.Discriminator}");
            MainWindow2.WD.Label_RPCStatus.Dispatcher.BeginInvoke((Action)delegate ()
            {
                MainWindow2.WD.EnableRun(true);
            });
        }

        public static bool FirstUpdate = false;
        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            if (FirstUpdate)
            {
                MainWindow2.SetLiveView(args);
                Log.Discord($"Updated presence");
            }
                FirstUpdate = true;
        }

        private static void Client_OnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            Log.Discord($"Error ({args.Code}) {args.Message}");
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            Fails++;
            if (Fails == 4)
            {
                Log.Discord("Failed to connect shutting down RPC");
                MainWindow.SetLiveView(GUI.ViewType.Error, "Discord client invalid");
                Fails = 0;
                
                try
                {
                    Shutdown();
                }
                catch { }
                MainWindow2.WD.Label_RPCStatus.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    MainWindow2.WD.DisableRun(true);
                });
            }
            else
                Log.Discord($"Failed connection {args.FailedPipe} {args.Type} | Attempt {Fails}");
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

