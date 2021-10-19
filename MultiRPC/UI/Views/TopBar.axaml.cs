﻿using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Rpc.Page;

namespace MultiRPC.UI.Views
{
    public partial class TopBar : UserControl
    {
        public TopBar()
        {
            InitializeComponent();
            if (RpcPageManager.CurrentPage != null)
            {
                RpcPageManagerOnPageChanged(this, RpcPageManager.CurrentPage);
            }
            RpcPageManager.PageChanged += delegate(object? sender, RpcPage page)
            {
                this.RunUILogic(() => RpcPageManagerOnPageChanged(sender, page));
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
                });
            };
            App.RpcClient.Ready += (sender, message) =>
            {
                rpcView.ViewType = ViewType.RichPresence;

                this.RunUILogic(() =>
                {
                    _startButton.ChangeJsonNames("Shutdown");
                    _statusKind.ChangeJsonNames("Connected");
                    btnUpdatePresence.IsEnabled = true;

                    //TODO: Temp with user storing
                    var user = message.User.Username + "#" + message.User.Discriminator.ToString("0000");
                    if (user != _user)
                    {
                        _user = user;
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
                });
            };
        }
        private readonly Language _statusKind;
        private readonly Language _statusText;
        private readonly Language _userText;
        private readonly Language _startButton;
        
        private void UpdateStatus()
        {
            this.RunUILogic(() => tblStatus.Text = _statusText.Text + ": " + _statusKind.Text);
        }

        //TODO: Store user's in settings
        private string _user = "NA#0000";
        private void UpdateUser()
        {
            this.RunUILogic(() => tblUser.Text = _userText.Text + ": " + _user);
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
                btnStart.Classes.Remove("red");
                btnStart.Classes.Add("green");
                return;
            }

            App.RpcClient.Start(null, null);
            App.RpcClient.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
            btnStart.Classes.Remove("green");
            btnStart.Classes.Add("red");
        }

        private void BtnUpdatePresence_OnClick(object? sender, RoutedEventArgs e)
        {
            App.RpcClient.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
        }
    }
}