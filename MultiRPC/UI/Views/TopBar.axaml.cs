using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DiscordRPC;
using MultiRPC.Extensions;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI.Views
{
    public partial class TopBar : UserControl
    {
        private readonly GeneralSettings _generalSettings = SettingManager<GeneralSettings>.Setting;

        private RpcPage _page;
        public TopBar()
        {
            InitializeComponent();
            if (RpcPageManager.CurrentPage != null)
            {
                _page = RpcPageManager.CurrentPage;
                RpcPageManagerOnPageChanged(this, _page);
                _page.PresenceChanged += PageOnPresenceChanged;
                PageOnPresenceChanged(this, EventArgs.Empty);
            }
            RpcPageManager.NewCurrentPage += delegate(object? sender, RpcPage page)
            {
                this.RunUILogic(() => RpcPageManagerOnPageChanged(sender, page));

                if (_page != null)
                {
                    _page.PresenceChanged -= PageOnPresenceChanged;
                }
                _page = page;
                page.PresenceChanged += PageOnPresenceChanged;
                PageOnPresenceChanged(sender, EventArgs.Empty);
            };

            btnStart.DataContext = _startButton = new Language("Start", "MultiRPC");
            btnUpdatePresence.DataContext = new Language("UpdatePresence");
            btnAfk.DataContext = new Language("Afk");
            btnAuto.DataContext = new Language("Auto");
            txtAfk.DataContext = new Language("AfkText");

            _statusKind = new Language("Disconnected");
            _statusText = new Language("Status");
            _userText = new Language("User");

            _statusKind.TextObservable.Subscribe(_ => UpdateStatus());
            _statusText.TextObservable.Subscribe(_ => UpdateStatus());
            _userText.TextObservable.Subscribe(x => UpdateUser());

            App.RpcClient.Loading += (sender, args) =>
            {
                rpcView.ViewType = ViewType.Loading;

                this.RunUILogic(() =>
                {
                    _startButton.ChangeJsonNames("Shutdown");
                    _statusKind.ChangeJsonNames("Loading");
                    btnUpdatePresence.IsEnabled = false;
                    btnStart.Classes.Remove("green");
                    btnStart.Classes.Add("red");

                    if (App.RpcClient.ID != Constants.AfkID)
                    {
                        btnAfk.IsEnabled = false;
                    }
                });
            };
            App.RpcClient.Ready += (sender, message) =>
            {
                rpcView.ViewType = ViewType.RpcRichPresence;

                this.RunUILogic(() =>
                {
                    _startButton.ChangeJsonNames("Shutdown");
                    _statusKind.ChangeJsonNames("Connected");
                    if (App.RpcClient.ID != Constants.AfkID)
                    {
                        btnUpdatePresence.IsEnabled = _page?.PresenceValid ?? true;
                    }

                    var user = message.User.Username + "#" + message.User.Discriminator.ToString("0000");
                    if (user != _generalSettings.LastUser)
                    {
                        _generalSettings.LastUser = user;
                        UpdateUser();
                    }
                });
            };
            App.RpcClient.Disconnected += (sender, args) =>
            {
                rpcView.ViewType = ViewType.Default;

                this.RunUILogic(() =>
                {
                    RpcPageManagerOnPageChanged(sender, RpcPageManager.CurrentPage!);
                    _statusKind.ChangeJsonNames("Disconnected");
                    btnUpdatePresence.IsEnabled = false;
                    btnStart.Classes.Remove("red");
                    btnStart.Classes.Add("green");
                    btnAfk.IsEnabled = true;
                });
            };
        }

        private void PageOnPresenceChanged(object? sender, EventArgs e)
        {
            if (!App.RpcClient.IsRunning)
            {
                btnStart.IsEnabled = _page.PresenceValid;
                return;
            }

            btnUpdatePresence.IsEnabled = _page.PresenceValid;
        }

        private readonly Language _statusKind;
        private readonly Language _statusText;
        private readonly Language _userText;
        private readonly Language _startButton;
        
        private void UpdateStatus()
        {
            this.RunUILogic(() => tblStatus.Text = _statusText.Text + ": " + _statusKind.Text);
        }

        private void UpdateUser()
        {
            this.RunUILogic(() => tblUser.Text = _userText.Text + ": " + _generalSettings.LastUser);
        }
        
        private void RpcPageManagerOnPageChanged(object? sender, RpcPage e)
        {
            var hasStartKey = Language.HasKey("Start" + e.LocalizableName);
            _startButton.ChangeJsonNames(hasStartKey ? new []{ "Start" + e.LocalizableName } : new []{ "Start", e.LocalizableName });
        }

        private void BtnStart_OnClick(object? sender, RoutedEventArgs e)
        {
            if (App.RpcClient.IsRunning)
            {
                App.RpcClient.Stop();
                return;
            }

            App.RpcClient.Start(null, null);
            App.RpcClient.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
        }

        private void BtnUpdatePresence_OnClick(object? sender, RoutedEventArgs e)
        {
            App.RpcClient.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
        }

        private void BtnAfk_OnClick(object? sender, RoutedEventArgs e)
        {
            var pre = new RichPresence
            {
                Details = txtAfk.Text,
                Assets = new Assets
                {
                    LargeImageKey = "cat",
                    LargeImageText = Language.GetText("SleepyCat")
                },
                Timestamps = _generalSettings.ShowAfkTime ? Timestamps.Now : null
            };

            if (App.RpcClient.IsRunning
                && App.RpcClient.ID != Constants.AfkID)
            {
                App.RpcClient.Stop();
            }

            if (!App.RpcClient.IsRunning)
            {
                App.RpcClient.Start(Constants.AfkID, "Afk");
            }
            App.RpcClient.UpdatePresence(pre);
            txtAfk.Clear();
        }
    }
}