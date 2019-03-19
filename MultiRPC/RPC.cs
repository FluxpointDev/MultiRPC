using System;
using System.IO;
using DiscordRPC;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MultiRPC.GUI.Pages;
using MultiRPC.GUI.Views;
using MultiRPC.JsonClasses;

namespace MultiRPC
{
    public static class RPC
    {
        public enum RPCType
        {
            MultiRPC,
            Custom
        }

        public static DiscordRpcClient RPCClient;
        private static System.Timers.Timer ClientTimer;
        private static DateTime StartTime;
        public static RPCType Type;
        private static System.Timers.Timer Uptime;
        public static RichPresence Presence = new RichPresence();
        public const ulong MuiltiRPCID = 450894077165043722;
        public static ulong IDToUse;
        private static string PageUserWasOnWhenStarted;

        /// <summary>
        /// Has rpc failed
        /// </summary>
        public static bool Failed = false;

        /// <summary>
        /// Is afk rpc status on
        /// </summary>
        public static bool AFK = false;

        public static async Task<bool> CheckPresence(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && !App.Config.InviteWarn && text.ToLower().Contains("discord.gg"))
            {
                var result = MessageBox.Show(App.Text.AdvertisingWarning, App.Text.Warning, MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    App.Config.InviteWarn = true;
                    App.Config.Save();
                    App.Logging.Application(App.Text.AdvertisingWarningDisabled);
                    return true;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public async static Task UpdateType(RPCType type)
        {
            if ((RPCClient != null && RPCClient.Disposed) || RPCClient == null)
                Type = type;
        }

        public static async void Start()
        {
            if(!await CheckPresence(Presence.Details) || !await CheckPresence(Presence.State))
                return;
            Failed = true;
            App.Logging.Application(App.Text.StartingRpc);
            int Count = 0;
            string DClient = "Discord";
            bool Found = false;

            if (App.Config.ClientToUse != 0)
            {
                foreach (string i in Directory.GetFiles(@"\\.\pipe\"))
                {
                    string[] Split = i.Split('\\');
                    string Pipe = Split.Last();

                    switch (Pipe)
                    {
                        case "discord-sock":
                            if (App.Config.ClientToUse == 1)
                            {
                                DClient = "Discord";
                                Found = true;
                            }
                            break;
                        case "discordptb-sock":
                            if (App.Config.ClientToUse == 2)
                            {
                                DClient = "Discord PTB";
                                Found = true;
                            }
                            break;
                        case "discordcanary-sock":
                            if (App.Config.ClientToUse == 3)
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

            App.Logging.Application($"Discord {App.Text.Client}: {DClient} ({Count})");           
            RPCClient = new DiscordRpcClient(IDToUse.ToString(), false, Count, App.Logging);

            RPCClient.OnConnectionEstablished += Client_OnConnectionEstablished;
            RPCClient.OnConnectionFailed += Client_OnConnectionFailed;
            RPCClient.OnPresenceUpdate += Client_OnPresenceUpdate;
            RPCClient.OnReady += Client_OnReady;

            MainPage.mainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
            {
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Loading);
            });
            MainPage.mainPage.butStart.Style = (Style)MainPage.mainPage.Resources["ButtonRed"];
            if(!AFK)
                MainPage.mainPage.butUpdate.IsEnabled = true;
            PageUserWasOnWhenStarted = MainPage.mainPage.butStart.Content.ToString();
            MainPage.mainPage.butStart.Content = App.Text.Shutdown;

            //Create a timer that will regularly call invoke
            ClientTimer = new System.Timers.Timer(150);
            ClientTimer.Elapsed += (sender, evt) => { RPCClient.Invoke(); };
            ClientTimer.Start();

            //Connect
            RPCClient.Initialize();
            //Send a presence. Do this as many times as you want
            StartTime = DateTime.UtcNow;
            if (Presence.Timestamps != null)
                Presence.Timestamps.Start = StartTime;

            MainPage.mainPage.Dispatcher.Invoke(() => MainPage.mainPage.rCon.Text = App.Text.Loading);
            RPCClient.SetPresence(Presence);
            foreach (var button in CustomPage.ProfileButtons)
                button.IsEnabled = false;
        }

        public static void SetPresence(string text1, string text2, string largeKey, string largeText, string smallKey, string smallText, bool showTime)
        {
            SetPresence(new CustomProfile
            {
                LargeKey = largeKey,
                Text1 = text1,
                Text2 = text2,
                LargeText = largeText,
                SmallKey = smallKey,
                SmallText = smallText,
                ShowTime = showTime
            });
        }

        public static void SetPresence(CustomProfile profile)
        {
            Presence.Details = profile.Text1;
            Presence.State = profile.Text2;
            Presence.Assets = !string.IsNullOrWhiteSpace(profile.LargeKey) || !string.IsNullOrWhiteSpace(profile.SmallKey) ? new Assets
            {
                LargeImageKey = profile.LargeKey,
                LargeImageText = profile.LargeText,
                SmallImageKey = profile.SmallKey,
                SmallImageText = profile.SmallText
            } : null;
            Presence.Timestamps = profile.ShowTime ? new Timestamps() : null;
        }

        public static void Update()
        {
            if (RPCClient != null && RPCClient.IsInitialized && !RPCClient.Disposed)
            {
                if (Presence.Timestamps != null)
                    Presence.Timestamps.Start = StartTime;
                RPCClient.SetPresence(Presence);
            }
            else
            {
                Start();
            }
        }

        private async static void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            string User = $"{args.User.Username}#{args.User.Discriminator}";
            MainPage.mainPage.Dispatcher.Invoke(() => MainPage.mainPage.rUsername.Text = User);
            if (App.Config.LastUser != User)
            {
                App.Config.LastUser = User;
                await App.Config.Save();
            }

            App.Logging.LogEvent(App.Text.Client, $"{App.Text.Hi} {User} 👋");
            
            Uptime = new System.Timers.Timer(new TimeSpan(0, 0, 1).TotalMilliseconds);
            if (Presence.Timestamps != null)
                Uptime.Start();
            Uptime.Elapsed += Uptime_Elapsed;
            MainPage.mainPage.Dispatcher.Invoke(() => MainPage.mainPage.rCon.Text = App.Text.Connected);
        }

        private static void Uptime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan ts = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - StartTime.Ticks);
            ts.Add(new TimeSpan(0, 0, 1));
            MainPage.mainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
            {
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateTime(ts);
            });
        }

        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            MainPage.mainPage.frameRPCPreview.Dispatcher.Invoke(() =>
            {
                MainPage.mainPage.frameRPCPreview.Content = new RPCPreview(args);
                if (Presence.Timestamps != null)
                    Uptime.Start();
                else
                    Uptime.Stop();
            });
            MainPage.mainPage.Dispatcher.Invoke(() => MainPage.mainPage.rCon.Text = App.Text.Connected);
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            if (!Failed)
            {
                Failed = true;
                MainPage.mainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
                {
                    await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Error, App.Text.AttemptingToReconnect);
                    MainPage.mainPage.rCon.Text = App.Text.Loading;
                });
            }
        }

        private static void Client_OnConnectionEstablished(object sender, DiscordRPC.Message.ConnectionEstablishedMessage args)
        {
            Failed = false;
        }

        public static void Shutdown()
        {
            App.Logging.LogEvent(App.Text.Client,App.Text.ShuttingDown);
            ClientTimer?.Dispose();
            Uptime?.Dispose();
            RPCClient?.ClearPresence();
            RPCClient?.Dispose();
            MainPage.mainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
            {
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Default);
            });

            MainPage.mainPage.butStart.Style = (Style)App.Current.Resources["ButtonGreen"];
            if (MainPage.mainPage.ContentFrame.Content is MultiRPCPage)
                MainPage.mainPage.butStart.Content = App.Text.Start + " MuiltiRPC";
            else if (MainPage.mainPage.ContentFrame.Content is CustomPage)
                MainPage.mainPage.butStart.Content = App.Text.StartCustom;
            else
                MainPage.mainPage.butStart.Content = PageUserWasOnWhenStarted;

            MainPage.mainPage.butUpdate.IsEnabled = false;
            MainPage.mainPage.rCon.Text = App.Text.Disconnected;
            foreach (var button in CustomPage.ProfileButtons)
                button.IsEnabled = true;
        }
    }
}
