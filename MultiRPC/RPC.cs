using System;
using System.IO;
using DiscordRPC;
using System.Linq;
using MultiRPC.GUI;
using System.Windows;
using MultiRPC.GUI.Pages;
using MultiRPC.GUI.Views;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;

namespace MultiRPC
{
    public static class RPC
    {
        public enum RPCType
        {
            MultiRPC,
            Custom
        }

        public static RPCType Type;
        public static ulong IDToUse;
        private static DateTime StartTime;
        public static DiscordRpcClient RPCClient;
        private static System.Timers.Timer Uptime;
        private static string PageUserWasOnWhenStarted;
        public const ulong MuiltiRPCID = 450894077165043722;
        public static RichPresence Presence = new RichPresence();


        /// <summary>
        /// Has rpc failed
        /// </summary>
        private static bool Failed = false;

        /// <summary>
        /// Is afk rpc status on
        /// </summary>
        public static bool AFK = false;

        public static async Task<bool> CheckPresence(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && !App.Config.InviteWarn && text.ToLower().Contains("discord.gg"))
            {
                var result = await CustomMessageBox.Show(App.Text.AdvertisingWarning, App.Text.Warning, MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    App.Config.InviteWarn = true;
                    await App.Config.Save();
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

        public static async Task Start()
        {
            //Check that the presence isn't ban worthy
            if (!await CheckPresence(Presence.Details) || !await CheckPresence(Presence.State))
                return;

            App.Logging.Application(App.Text.StartingRpc);

            Failed = true;
            int count = 0;
            string DClient = "Discord";
            bool found = false;

            //Get the client to connect to
            if (App.Config.ClientToUse != 0)
            {
                foreach (string i in Directory.GetFiles(@"\\.\pipe\"))
                {
                    string pipe = i.Split('\\').Last();

                    switch (pipe)
                    {
                        case "discord-sock":
                            if (App.Config.ClientToUse == 1)
                            {
                                DClient = "Discord";
                                found = true;
                            }
                            break;
                        case "discordptb-sock":
                            if (App.Config.ClientToUse == 2)
                            {
                                DClient = "Discord PTB";
                                found = true;
                            }
                            break;
                        case "discordcanary-sock":
                            if (App.Config.ClientToUse == 3)
                            {
                                DClient = "Discord Canary";
                                found = true;
                            }
                            break;
                    }
                    if (found)
                        break;
                    else if (pipe.StartsWith("discord"))
                        count++;
                }
            }

            //Log the client we are going to connect to
            App.Logging.Application($"Discord {App.Text.Client}: {DClient} ({count})");           
            RPCClient = new DiscordRpcClient(IDToUse.ToString(), count, App.Logging, true);

            //Set up events
            RPCClient.OnConnectionEstablished += Client_OnConnectionEstablished;
            RPCClient.OnConnectionFailed += Client_OnConnectionFailed;
            RPCClient.OnPresenceUpdate += Client_OnPresenceUpdate;
            RPCClient.OnReady += Client_OnReady;

            if (!AFK)
                MainPage.mainPage.btnUpdate.IsEnabled = true;

            //Show that we are going to load things™
            await MainPage.mainPage.frameRPCPreview.Dispatcher.InvokeAsync(async () =>
            {
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Loading);
                MainPage.mainPage.rCon.Text = App.Text.Loading;
                MainPage.mainPage.btnStart.Style = (Style)MainPage.mainPage.Resources["ButtonRed"];
                PageUserWasOnWhenStarted = MainPage.mainPage.btnStart.Content.ToString();
                MainPage.mainPage.btnStart.Content = App.Text.Shutdown;
            });

            //Connect
            RPCClient.Initialize();

            //Set up the time that will show (if user wants it)
            StartTime = DateTime.UtcNow;
            if (Presence.Timestamps != null)
                Presence.Timestamps.Start = StartTime;
            //Send the presence
            RPCClient.SetPresence(Presence);

            //Disable buttons
            foreach (var button in CustomPage.ProfileButtons)
                button.IsEnabled = false;

            CustomPage.customPage.imgProfileAdd.IsEnabled = false;
            CustomPage.customPage.imgProfileDelete.IsEnabled = false;
            CustomPage.customPage.tbClientID.IsEnabled = false;
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

        public static async void Update()
        {
            if (RPCClient != null && RPCClient.IsInitialized && !RPCClient.Disposed)
            {
                if (Presence.Timestamps != null)
                    Presence.Timestamps.Start = StartTime;
                RPCClient.SetPresence(Presence);
            }
            else
            {
                await Start();
            }
        }

        private static async void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            string user = $"{args.User.Username}#{args.User.Discriminator.ToString("0000")}";
            if (App.Config.LastUser != user)
            {
                App.Config.LastUser = user;
                await App.Config.Save();
            }

            App.Logging.LogEvent(App.Text.Client, $"{App.Text.Hi} {user} 👋");

            //Make update timer to update time in GUI
            Uptime = new System.Timers.Timer(new TimeSpan(0, 0, 1).TotalMilliseconds);
            if (Presence.Timestamps != null)
                Uptime.Start();
            Uptime.Elapsed += Uptime_Elapsed;

            MainPage.mainPage.Dispatcher.Invoke(() =>
            {
                MainPage.mainPage.rUsername.Text = user;
                MainPage.mainPage.rCon.Text = App.Text.Connected;
            });
        }

        private static void Uptime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan ts = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - StartTime.Ticks + TimeSpan.TicksPerSecond);
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

            Uptime?.Dispose();
            RPCClient?.Dispose();
            MainPage.mainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
            {
                await ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Default);
            });

            MainPage.mainPage.btnStart.Style = (Style)App.Current.Resources["ButtonGreen"];
            if (MainPage.mainPage.ContentFrame.Content is MultiRPCPage multiRpcPage)
            {
                MainPage.mainPage.btnStart.Content = App.Text.Start + " MuiltiRPC";
                multiRpcPage.CanRunRPC();
            }
            else if (MainPage.mainPage.ContentFrame.Content is CustomPage customPage)
            {
                MainPage.mainPage.btnStart.Content = App.Text.StartCustom;
                customPage.CanRunRPC();
            }
            else
            {
                MainPage.mainPage.btnStart.Content = PageUserWasOnWhenStarted;
            }

            MainPage.mainPage.btnUpdate.IsEnabled = false;
            MainPage.mainPage.rCon.Text = App.Text.Disconnected;

            foreach (var button in CustomPage.ProfileButtons)
                button.IsEnabled = true;
            CustomPage.customPage.imgProfileAdd.IsEnabled = true;
            CustomPage.customPage.imgProfileDelete.IsEnabled = true;
            CustomPage.customPage.tbClientID.IsEnabled = true;
        }
    }
}
