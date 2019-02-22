using DiscordRPC;
using MultiRPC.Data;
using MultiRPC.Functions;
using MultiRPC.GUI;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using MultiRPC.GUI.Pages;

namespace MultiRPC
{
    public static class RPC
    {
        public static HttpClient HttpClient = new HttpClient
        {
            Timeout = new TimeSpan(0, 0, 3)
        };
        public static DiscordRpcClient Client = null;
        private static System.Timers.Timer ClientTimer;
        public static System.Timers.Timer Uptime;
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
            if (!App.Config.InviteWarn && text.ToLower().Contains("discord.gg/"))
            {
                App.Config.InviteWarn = true;
                App.Config.Save();
                MessageBox.Show("Advertising in rpc could result in you being kicked or banned from servers!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.Log.App("Disabled invite warning message");
            }
        }

        public static void Start(ulong id, string SteamID = "")
        {
            FirstUpdate = false;
            Failed = true;
            App.Log.Rpc("Starting MultiRPC");
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
            App.Log.App($"Client: {DClient} ({Count})");
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

        public static void SetPresence(CustomProfile profile)
        {
            Presence.Details = profile.Text1;
            Presence.State = profile.Text2;
            Presence.Assets = new Assets
            {
                LargeImageKey = profile.LargeKey,
                LargeImageText = profile.LargeText,
                SmallImageKey = profile.SmallKey,
                SmallImageText = profile.SmallText
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
            string[] Split = args.User.ToString().Split('#');
            string User = $"{Split[0]}#{Split[1].PadLeft(4, '0')}";
            if (App.Config.LastUser != User)
            {
                App.Config.LastUser = User;
                FuncCredits.CheckBadges();
            }
            App.Log.Rpc($"Ready, hi {User} 👋");
            StartTime = DateTime.UtcNow;
            Uptime = new System.Timers.Timer(new TimeSpan(0, 0, 1).TotalMilliseconds);
            if (Presence.Timestamps != null)
                Uptime.Start();
            Uptime.Elapsed += Uptime_Elapsed;
            App.WD.TextUser.Dispatcher.BeginInvoke((Action)delegate ()
            {
                if (!AFK)
                    App.WD.BtnUpdatePresence.IsEnabled = true;
                App.WD.TextUser.Content = User;
                MainPage.DisableElements(true);
            });
        }

        public static DateTime StartTime;
        private static void Uptime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan TS = DateTime.UtcNow - StartTime;
            TS.Add(new TimeSpan(0, 0, 1));
            App.WD.FrameLiveRPC.Dispatcher.Invoke(() =>
            {
                ViewRPC View = App.WD.FrameLiveRPC.Content as ViewRPC;
                if (TS.Hours == 0)
                    View.Time.Content = $"{TS.Minutes.ToString().PadLeft(2, '0')}:{TS.Seconds.ToString().PadLeft(2, '0')}";
                else
                    View.Time.Content = $"{TS.Hours.ToString().PadLeft(2, '0')}:{TS.Minutes.ToString().PadLeft(2, '0')}:{TS.Seconds.ToString().PadLeft(2, '0')}";
            });

        }


        public static bool FirstUpdate = false;
        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            if (FirstUpdate)
            {
                App.Log.Rpc($"Updated presence for {args.Name}");
                MainPage.SetLiveView(args);
            }
            FirstUpdate = true;
        }

        private static void Client_OnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            App.Log.Error("RPC", $"({args.Code}) {args.Message}");
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            if (!Failed)
            {
                Failed = true;
                App.Log.Error("RPC", $"Discord client invalid, {args.Type}");
                MainPage.SetLiveView(ViewType.Error, "Attempting to reconnect");
            }
        }

        private static void Client_OnConnectionEstablished(object sender, DiscordRPC.Message.ConnectionEstablishedMessage args)
        {
            Failed = false;
            App.Log.Rpc($"Connected");
        }

        private static void Client_OnClose(object sender, DiscordRPC.Message.CloseMessage args)
        {
            App.Log.Rpc($"Closed ({args.Code}) {args.Reason}");
        }

        public static void Shutdown()
        {
            App.Log.Rpc("Shutting down");
            if (Presence != null)
                Presence.Timestamps = null;
            ClientTimer?.Dispose();
            Uptime?.Dispose();
            Client?.Dispose();
        }
    }
}

