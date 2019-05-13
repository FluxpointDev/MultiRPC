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
using System.Windows.Controls;

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
                var pipes = Directory.GetFiles(@"\\.\pipe\");
                for (int i = 0; i < pipes.Length; i++)
                {
                    string pipe = pipes[i].Split('\\').Last();

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
                MainPage._MainPage.btnUpdate.IsEnabled = true;

            //Show that we are going to load things™
            await MainPage._MainPage.frameRPCPreview.Dispatcher.InvokeAsync(async () =>
            {
                await ((RPCPreview)MainPage._MainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Loading);
                MainPage._MainPage.rCon.Text = App.Text.Loading;
                MainPage._MainPage.btnStart.Style = (Style)MainPage._MainPage.Resources["ButtonRed"];
                PageUserWasOnWhenStarted = MainPage._MainPage.btnStart.Content.ToString();
                MainPage._MainPage.btnStart.Content = App.Text.Shutdown;
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
            string profileClientID = "0";

            if(!AFK && CustomPage.CurrentButton != null)
                profileClientID = CustomPage.Profiles[CustomPage.CurrentButton.Content.ToString()]
                    .ClientID;
            for (int i = 0; i < CustomPage.ProfileButtons.Count; i++)
            {
                if (profileClientID == "0" || CustomPage.Profiles[CustomPage.ProfileButtons[i].Content.ToString()]
                        .ClientID != profileClientID)
                    CustomPage.ProfileButtons[i].IsEnabled = false;
            }

            CustomPage._CustomPage.imgProfileAdd.IsEnabled = false;
            CustomPage._CustomPage.imgProfileDelete.IsEnabled = false;
            CustomPage._CustomPage.tbClientID.IsEnabled = false;
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

            MainPage._MainPage.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.rUsername.Text = user;
                MainPage._MainPage.rCon.Text = App.Text.Connected;
            });
        }

        private static void Uptime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan ts = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - StartTime.Ticks + TimeSpan.TicksPerSecond);
            MainPage._MainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
            {
                await ((RPCPreview)MainPage._MainPage.frameRPCPreview.Content).UpdateTime(ts);
            });
        }

        private static void Client_OnPresenceUpdate(object sender, DiscordRPC.Message.PresenceMessage args)
        {
            MainPage._MainPage.frameRPCPreview.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.frameRPCPreview.Content = new RPCPreview(args);
                if (Presence.Timestamps != null)
                    Uptime.Start();
                else
                    Uptime.Stop();
            });
            MainPage._MainPage.Dispatcher.Invoke(() => MainPage._MainPage.rCon.Text = App.Text.Connected);
        }

        private static void Client_OnConnectionFailed(object sender, DiscordRPC.Message.ConnectionFailedMessage args)
        {
            if (!Failed)
            {
                Failed = true;
                MainPage._MainPage.frameRPCPreview.Dispatcher.Invoke( async () =>
                {
                    await ((RPCPreview)MainPage._MainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Error, App.Text.AttemptingToReconnect);
                    MainPage._MainPage.rCon.Text = App.Text.Loading;
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
            MainPage._MainPage.frameRPCPreview.Dispatcher.Invoke(async () =>
            {
                await ((RPCPreview)MainPage._MainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Default);
            });

            MainPage._MainPage.btnStart.SetResourceReference(Button.StyleProperty, "ButtonGreen");
            if (MainPage._MainPage.ContentFrame.Content is MultiRPCPage multiRpcPage)
            {
                MainPage._MainPage.btnStart.Content = App.Text.Start + " MuiltiRPC";
                multiRpcPage.CanRunRPC();
            }
            else if (MainPage._MainPage.ContentFrame.Content is CustomPage _CustomPage)
            {
                MainPage._MainPage.btnStart.Content = App.Text.StartCustom;
                _CustomPage.CanRunRPC();
            }
            else
            {
                MainPage._MainPage.btnStart.Content = PageUserWasOnWhenStarted;
            }

            MainPage._MainPage.btnUpdate.IsEnabled = false;
            MainPage._MainPage.rCon.Text = App.Text.Disconnected;
            AFK = false;

            for (int i = 0; i < CustomPage.ProfileButtons.Count; i++)
            {
                CustomPage.ProfileButtons[i].IsEnabled = true;
            }
            CustomPage._CustomPage.imgProfileAdd.IsEnabled = true;
            CustomPage._CustomPage.imgProfileDelete.IsEnabled = true;
            CustomPage._CustomPage.tbClientID.IsEnabled = true;
        }
    }
}
