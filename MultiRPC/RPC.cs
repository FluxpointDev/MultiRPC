using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using DiscordRPC;
using DiscordRPC.Message;
using MultiRPC.GUI;
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

        public const ulong MultiRPCID = 450894077165043722;

        public static RPCType Type;
        public static RichPresence Presence = new RichPresence();

        private static DateTime _startTime;
        private static Timer _uptime;

        public static ulong IDToUse;
        private static DiscordRpcClient RPCClient;
        private static string _pageUserWasOnWhenStarted;

        /// <summary>
        ///     Has rpc failed
        /// </summary>
        private static bool _failed;

        /// <summary>
        ///     Is afk rpc status on
        /// </summary>
        public static bool AFK;

        public static bool IsRPCRunning { get; private set; }

        private static async Task<bool> CheckPresence(string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && !App.Config.InviteWarn && text.ToLower().Contains("discord.gg"))
            {
                var result = await CustomMessageBox.Show(App.Text.AdvertisingWarning, App.Text.Warning,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    App.Config.InviteWarn = true;
                    await App.Config.Save();
                    App.Logging.Application(App.Text.AdvertisingWarningDisabled);
                    return true;
                }

                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Equals(this RichPresence richPresence, CustomProfile profile)
        {
            if (richPresence.Details != profile.Text1)
            {
                return false;
            }
            if (richPresence.State != profile.Text2)
            {
                return false;
            }
            if (richPresence.Timestamps == null && profile.ShowTime || richPresence.Timestamps != null && !profile.ShowTime)
            {
                return false;
            }

            var assets = !string.IsNullOrWhiteSpace(profile.LargeKey) || !string.IsNullOrWhiteSpace(profile.SmallKey)
                ? new Assets
                {
                    LargeImageKey = profile.LargeKey,
                    LargeImageText = profile.LargeText,
                    SmallImageKey = profile.SmallKey,
                    SmallImageText = profile.SmallText
                }
                : null;
            if (richPresence.Assets != assets)
            {
                return false;
            }

            return true;
        }

        public static Task UpdateType(RPCType type)
        {
            if (!IsRPCRunning)
            {
                Type = type;
            }

            return Task.CompletedTask;
        }

        public static async Task Start()
        {
            //Check that the presence isn't ban worthy
            if (!await CheckPresence(Presence.Details) || !await CheckPresence(Presence.State))
            {
                return;
            }

            App.Logging.Application(App.Text.StartingRpc);

            _failed = true;
            var pipeCount = 0;
            var discordClientName = "Discord";
            var foundClient = false;

            //Get the client to connect to
            if (App.Config.ClientToUse != 0)
            {
                var pipes = Directory.GetFiles(@"\\.\pipe\");
                for (var i = 0; i < pipes.Length; i++)
                {
                    var pipe = pipes[i].Split('\\').Last();

                    switch (pipe)
                    {
                        case "discord-sock":
                            if (App.Config.ClientToUse == 1)
                            {
                                discordClientName = "Discord";
                                foundClient = true;
                            }

                            break;
                        case "discordptb-sock":
                            if (App.Config.ClientToUse == 2)
                            {
                                discordClientName = "Discord PTB";
                                foundClient = true;
                            }

                            break;
                        case "discordcanary-sock":
                            if (App.Config.ClientToUse == 3)
                            {
                                discordClientName = "Discord Canary";
                                foundClient = true;
                            }

                            break;
                    }

                    if (foundClient)
                    {
                        break;
                    }

                    if (pipe.StartsWith("discord"))
                    {
                        pipeCount++;
                    }
                }
            }

            //Log the client we are going to connect to
            App.Logging.Application($"Discord {App.Text.Client}: {discordClientName} ({pipeCount})");
            RPCClient = new DiscordRpcClient(IDToUse.ToString(), pipeCount, App.Logging, true);

            //Set up events
            RPCClient.OnConnectionEstablished += Client_OnConnectionEstablished;
            RPCClient.OnConnectionFailed += Client_OnConnectionFailed;
            RPCClient.OnPresenceUpdate += Client_OnPresenceUpdate;
            RPCClient.OnReady += Client_OnReady;

            if (!AFK)
            {
                MainPage._MainPage.btnUpdate.IsEnabled = true;
            }

            //Show that we are going to load things™
            await MainPage._MainPage.frmRPCPreview.Dispatcher.InvokeAsync(async () =>
            {
                await ((RPCPreview) MainPage._MainPage.frmRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType
                    .Loading);
                MainPage._MainPage.rCon.Text = App.Text.Loading;
                MainPage._MainPage.btnStart.Style = (Style) MainPage._MainPage.Resources["ButtonRed"];
                _pageUserWasOnWhenStarted = MainPage._MainPage.btnStart.Content.ToString();
                MainPage._MainPage.btnStart.Content = App.Text.Shutdown;
            });

            //Connect
            RPCClient.Initialize();

            //Set up the time that will show (if user wants it)
            _startTime = DateTime.UtcNow;
            if (Presence.Timestamps != null)
            {
                Presence.Timestamps.Start = _startTime;
            }

            //Send the presence
            RPCClient.SetPresence(Presence);

            if (MasterCustomPage._MasterCustomPage != null)
            {
                //Disable buttons unless it's the same ClientID (still not allowed to mess with the Client ID tho)
                var profileClientID = !AFK && MasterCustomPage.CurrentButton != null &&
                                      MainPage._MainPage.frmContent.Content is MasterCustomPage
                    ? MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].ClientID
                    : "0";

                for (var i = 0; i < MasterCustomPage.ProfileButtons.Count; i++)
                {
                    if (profileClientID == "0" || MasterCustomPage.Profiles[MasterCustomPage.ProfileButtons[i].Content.ToString()]
                            .ClientID != profileClientID)
                    {
                        MasterCustomPage.ProfileButtons[i].IsEnabled = false;
                    }
                }

                MasterCustomPage._MasterCustomPage.imgProfileAdd.IsEnabled = false;
                MasterCustomPage._MasterCustomPage.imgProfileDelete.IsEnabled = false;
                CustomPage._CustomPage.tbClientID.IsEnabled = false;
            }

            IsRPCRunning = true;
        }

        public static void SetPresence(string text1, string text2, string largeKey, string largeText, string smallKey,
            string smallText, bool showTime)
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

        private static void SetPresence(CustomProfile profile)
        {
            if (AFK)
            {
                return;
            }

            Presence.Details = profile.Text1;
            Presence.State = profile.Text2;
            Presence.Assets =
                !string.IsNullOrWhiteSpace(profile.LargeKey) || !string.IsNullOrWhiteSpace(profile.SmallKey)
                    ? new Assets
                    {
                        LargeImageKey = profile.LargeKey,
                        LargeImageText = profile.LargeText,
                        SmallImageKey = profile.SmallKey,
                        SmallImageText = profile.SmallText
                    }
                    : null;
            Presence.Timestamps = profile.ShowTime ? new Timestamps() : null;
        }

        public static async void Update()
        {
            if (IsRPCRunning)
            {
                if (Presence.Timestamps != null)
                {
                    Presence.Timestamps.Start = _startTime;
                }

                RPCClient.SetPresence(Presence);
            }
            else
            {
                await Start();
            }
        }

        private static async void Client_OnReady(object sender, ReadyMessage args)
        {
            var user = $"{args.User.Username}#{args.User.Discriminator.ToString("0000")}";
            if (App.Config.LastUser != user)
            {
                App.Config.LastUser = user;
                await App.Config.Save();
            }

            App.Logging.LogEvent(App.Text.Client, $"{App.Text.Hi} {user} 👋");

            //Make update timer to update time in GUI
            _uptime = new Timer(new TimeSpan(0, 0, 1).TotalMilliseconds);
            if (Presence.Timestamps != null)
            {
                _uptime.Start();
            }

            _uptime.Elapsed += Uptime_Elapsed;

            MainPage._MainPage.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.rUsername.Text = user;
                MainPage._MainPage.rCon.Text = App.Text.Connected;
            });
        }

        private static void Uptime_Elapsed(object sender, ElapsedEventArgs e)
        {
            var ts = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _startTime.Ticks);
            MainPage._MainPage.frmRPCPreview.Dispatcher.Invoke(async () =>
            {
                await ((RPCPreview) MainPage._MainPage.frmRPCPreview.Content).UpdateTime(ts);
            });
        }

        private static void Client_OnPresenceUpdate(object sender, PresenceMessage args)
        {
            MainPage._MainPage.frmRPCPreview.Dispatcher.Invoke(() =>
            {
                MainPage._MainPage.frmRPCPreview.Content = new RPCPreview(args);
                if (Presence.Timestamps != null)
                {
                    _uptime.Start();
                }
                else
                {
                    _uptime.Stop();
                }

                MainPage._MainPage.rCon.Text = App.Text.Connected;
            });
        }

        private static void Client_OnConnectionFailed(object sender, ConnectionFailedMessage args)
        {
            if (!_failed)
            {
                _failed = true;
                MainPage._MainPage.frmRPCPreview.Dispatcher.Invoke(async () =>
                {
                    await ((RPCPreview) MainPage._MainPage.frmRPCPreview.Content).UpdateUIViewType(
                        RPCPreview.ViewType.Error, App.Text.AttemptingToReconnect);
                    MainPage._MainPage.rCon.Text = App.Text.Loading;
                });
            }
        }

        private static void Client_OnConnectionEstablished(object sender, ConnectionEstablishedMessage args)
        {
            _failed = false;
        }

        public static void Shutdown()
        {
            App.Logging.LogEvent(App.Text.Client, App.Text.ShuttingDown);

            _uptime?.Stop();
            _uptime?.Dispose();
            RPCClient?.Dispose();
            MainPage._MainPage.frmRPCPreview.Dispatcher.Invoke(async () =>
            {
                await ((RPCPreview) MainPage._MainPage.frmRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType
                    .Default);

                MainPage._MainPage.btnStart.SetResourceReference(FrameworkElement.StyleProperty, "ButtonGreen");
                switch (MainPage._MainPage.frmContent.Content)
                {
                    case MultiRPCPage multiRpcPage:
                        MainPage._MainPage.btnStart.Content = App.Text.Start + " MultiRPC";
                        multiRpcPage.CanRunRPC();
                        break;
                    case MasterCustomPage _CustomPage:
                        MainPage._MainPage.btnStart.Content = App.Text.StartCustom;
                        _CustomPage.CustomPage.CanRunRPC();
                        break;
                    default:
                        MainPage._MainPage.btnStart.Content = _pageUserWasOnWhenStarted.Contains("MultiRPC")
                            ? App.Text.Start + " MultiRPC"
                            : App.Text.StartCustom;
                        break;
                }

                MainPage._MainPage.btnUpdate.IsEnabled = false;
                MainPage._MainPage.rCon.Text = App.Text.Disconnected;
            });

            AFK = false;

            MasterCustomPage._MasterCustomPage.Dispatcher.Invoke(() =>
            {
                if (MasterCustomPage._MasterCustomPage != null)
                {
                    for (var i = 0; i < MasterCustomPage.ProfileButtons.Count; i++)
                    {
                        MasterCustomPage.ProfileButtons[i].IsEnabled = true;
                    }

                    MasterCustomPage._MasterCustomPage.imgProfileAdd.IsEnabled = true;
                    MasterCustomPage._MasterCustomPage.imgProfileDelete.IsEnabled = true;
                }
            });
            CustomPage._CustomPage.Dispatcher.Invoke(() => CustomPage._CustomPage.tbClientID.IsEnabled = true);

            IsRPCRunning = false;
        }
    }
}