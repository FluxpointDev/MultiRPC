using Avalonia.Controls;
using Avalonia.Interactivity;
using DiscordRPC;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using Splat;
using RichPresence = DiscordRPC.RichPresence;

namespace MultiRPC.UI.Views;

public partial class TopBar : Grid
{
    private readonly GeneralSettings _generalSettings = SettingManager<GeneralSettings>.Setting;
    private IRpcPage? _page;
    private readonly RpcClient _rpcClient;
    private readonly Language _statusKind;
    private readonly Language _statusText;
    private readonly Language _userText;
    private readonly Language _startButton;
    
    public TopBar()
    {
        InitializeComponent();
        _rpcClient = Locator.Current.GetService<RpcClient>() ?? throw new NoRpcClientException();
        if (RpcPageManager.CurrentPage != null)
        {
            _page = RpcPageManager.CurrentPage;
            RpcPageManagerOnPageChanged(this, _page);
            _page.PresenceChanged += PageOnPresenceChanged;
            PageOnPresenceChanged(this, EventArgs.Empty);
        }
        
        RpcPageManager.PageChanged += (sender, page) => btnUpdatePresence.IsEnabled = page == RpcPageManager.CurrentPage;
        RpcPageManager.NewCurrentPage += delegate(object? sender, IRpcPage page)
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

        btnStart.DataContext = _startButton = new Language(LanguageText.Start, LanguageText.MultiRPC);
        btnUpdatePresence.DataContext = (Language)LanguageText.UpdatePresence;
        btnAfk.DataContext = (Language)LanguageText.Afk;
        btnAuto.DataContext = (Language)LanguageText.Auto;
        txtAfk.DataContext = (Language)LanguageText.AfkText;

        _statusKind = new Language(LanguageText.Disconnected);
        _statusText = LanguageText.Status;
        _userText = LanguageText.User;

        _statusKind.TextObservable.Subscribe(_ => UpdateStatus());
        _statusText.TextObservable.Subscribe(_ => UpdateStatus());
        _userText.TextObservable.Subscribe(_ => UpdateUser());

        _rpcClient.Loading += (sender, args) =>
        {
            this.RunUILogic(() =>
            {
                rpcView.ViewType = ViewType.Loading;
                _startButton.ChangeJsonNames(LanguageText.Shutdown);
                _statusKind.ChangeJsonNames(LanguageText.Loading);
                btnUpdatePresence.IsEnabled = false;
                btnStart.Classes.Remove("green");
                btnStart.Classes.Add("red");

                if (_rpcClient.ID != Constants.AfkID)
                {
                    btnAfk.IsEnabled = false;
                }
            });
        };
        _rpcClient.Ready += (sender, message) =>
        {
            this.RunUILogic(() =>
            {
                rpcView.ViewType = ViewType.RpcRichPresence;
                _startButton.ChangeJsonNames(LanguageText.Shutdown);
                _statusKind.ChangeJsonNames(LanguageText.Connected);
                if (_rpcClient.ID != Constants.AfkID)
                {
                    btnUpdatePresence.IsEnabled = _page == RpcPageManager.PendingPage && (_page?.PresenceValid ?? true);
                }

                var user = message.User.Username + "#" + message.User.Discriminator.ToString("0000");
                if (user != _generalSettings.LastUser)
                {
                    _generalSettings.LastUser = user;
                    UpdateUser();
                }
            });
        };
        _rpcClient.Disconnected += (sender, args) =>
        {
            this.RunUILogic(() =>
            {
                rpcView.ViewType = ViewType.Default;
                RpcPageManagerOnPageChanged(sender, RpcPageManager.CurrentPage!);
                _statusKind.ChangeJsonNames(LanguageText.Disconnected);
                btnUpdatePresence.IsEnabled = false;
                btnStart.Classes.Remove("red");
                btnStart.Classes.Add("green");
                btnStart.IsEnabled = _page?.PresenceValid ?? true;
                btnAfk.IsEnabled = true;
            });
        };
    }

    private void PageOnPresenceChanged(object? sender, EventArgs e)
    {
        if (!_rpcClient.IsRunning)
        {
            btnStart.IsEnabled = _page?.PresenceValid ?? true;
            return;
        }

        btnUpdatePresence.IsEnabled = _page?.PresenceValid ?? true;
    }

    private void UpdateStatus()
    {
        this.RunUILogic(() => tblStatus.Text = _statusText.Text + ": " + _statusKind.Text);
    }

    private void UpdateUser()
    {
        this.RunUILogic(() => tblUser.Text = _userText.Text + ": " + _generalSettings.LastUser);
    }
        
    private void RpcPageManagerOnPageChanged(object? sender, IRpcPage e)
    {
        var hasStartKey = Language.HasKey("Start" + e.LocalizableName);
        _startButton.ChangeJsonNames(hasStartKey ? 
            new []{ "Start" + e.LocalizableName } 
            : new []{ "Start", e.LocalizableName });
    }

    public void TriggerStartStop() => BtnStart_OnClick(btnStart, null!);

    private async void BtnStart_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_rpcClient.IsRunning)
        {
            _rpcClient.Stop();
            return;
        }

        _rpcClient.Start(null, null);
        await _rpcClient.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
    }

    private async void BtnUpdatePresence_OnClick(object? sender, RoutedEventArgs e)
    {
        await _rpcClient.UpdatePresence(RpcPageManager.CurrentPage?.RichPresence);
    }

    private async void BtnAfk_OnClick(object? sender, RoutedEventArgs e)
    {
        var pre = new RichPresence
        {
            Details = txtAfk.Text,
            Assets = new Assets
            {
                LargeImageKey = "cat",
                LargeImageText = Language.GetText(LanguageText.SleepyCat)
            },
            Timestamps = _generalSettings.ShowAfkTime ? Timestamps.Now : null
        };

        if (_rpcClient.IsRunning
            && _rpcClient.ID != Constants.AfkID)
        {
            _rpcClient.Stop();
        }

        if (!_rpcClient.IsRunning)
        {
            _rpcClient.Start(Constants.AfkID, Language.GetText(LanguageText.Afk));
        }
        await _rpcClient.UpdatePresence(pre);
        txtAfk.Clear();
    }
}