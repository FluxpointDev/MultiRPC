using DiscordRPC;
using MultiRPC.GUI;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace MultiRPC
{
    public static class RPC
    {

        public static Config Config = new Config();
        public static string ConfigFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC/";
        public static string ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC/Config.json";
        public static HttpClient HttpClient = new HttpClient
        {
            Timeout = new TimeSpan(0, 0, 3)
        };
        public static Logger Log = new Logger();
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
                Log.App("Disabled invite warning message");
            }
        }

        public static void Start(ulong id, string SteamID = "")
        {
            FirstUpdate = false;
            Failed = true;
            Log.Rpc("Starting MultiRPC");
            int Count = 0;
            string DClient = "";
            bool Found = false;
            if (App.WD.ItemsPipe.SelectedIndex != 0)
            {
                foreach (string i in Directory.GetFiles(@"\\.\pipe\"))
                {
                    string[] Split = i.Split('\\');
                    string Pipe = Split.Last();

                    switch (Pipe)
                    {
                        case "discord-sock":
                            if (App.WD.ItemsPipe.SelectedIndex == 1)
                            {
                                DClient = "Discord";
                                Found = true;
                            }
                            break;
                        case "discordptb-sock":
                            if (App.WD.ItemsPipe.SelectedIndex == 2)
                            {
                                DClient = "Discord PTB";
                                Found = true;
                            }
                            break;
                        case "discordcanary-sock":
                            if (App.WD.ItemsPipe.SelectedIndex == 3)
                            {
                                DClient = "Discord Canary";
                                Found = true;
                            }
                            break;
                    }
                    if (Found)
                        break;
                    else if (Pipe.StartsWith("discord"))
                        Count++;
                }
            }
            Log.App($"Client: {DClient} ({Count})");
            if (SteamID == "")
                Client = new DiscordRpcClient(id.ToString(), false, Count);
            else
                Client = new DiscordRpcClient(id.ToString(), SteamID, false, Count);
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
            Log.Rpc($"Ready, hi {args.User.Username}#{args.User.Discriminator} 👋");
            App.WD.TextUser.Dispatcher.BeginInvoke((Action)delegate ()
            {
                App.WD.TextUser.Content = args.User.ToString();
                MainWindow.DisableElements(true);
            });

            App.WD.TextStatus.Dispatcher.BeginInvoke((Action)delegate ()
            {
                MainWindow.DisableElements(true);
            });
        }

        public static bool FirstUpdate = false;
        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            if (FirstUpdate)
            {
                Log.Rpc($"Updated presence for {args.Name}");
                MainWindow.SetLiveView(args);
            }
            FirstUpdate = true;
        }

        private static void Client_OnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            Log.Error("RPC", $"({args.Code}) {args.Message}");
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            if (!Failed)
            {
                Failed = true;
                Log.Error("RPC", $"Discord client invalid, {args.Type}");
                MainWindow.SetLiveView(ViewType.Error, "Attempting to reconnect");
            }
        }

        private static void Client_OnConnectionEstablished(object sender, DiscordRPC.Message.ConnectionEstablishedMessage args)
        {
            Failed = false;
            Log.Rpc($"Connected");
        }

        private static void Client_OnClose(object sender, DiscordRPC.Message.CloseMessage args)
        {
            Log.Rpc($"Closed ({args.Code}) {args.Reason}");
        }

        public static void Shutdown()
        {
            Log.Rpc("Shutting down");
            ClientTimer.Dispose();
            Client.Dispose();
        }

    }
}

