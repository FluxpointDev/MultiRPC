using DiscordRPC;
using DiscordRPC.Message;
using MultiRPC.Logging;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI;
using MultiRPC.Utils;
using TinyUpdate.Core.Logging;

namespace MultiRPC.Rpc;

public class RpcClient
{
    private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(RpcClient));
    private DateTime _rpcStart;
    private string _presenceName = Language.GetText(LanguageText.Unknown);
    private long _presenceId;
    private DiscordRpcClient? _client;
    private readonly DisableSettings _disableSettings = SettingManager<DisableSettings>.Setting;

    public bool IsRunning => Status != ConnectionStatus.Disconnected;
    public long ID => _presenceId;
    public ConnectionStatus Status { get; private set; }

    public event EventHandler<ReadyMessage>? Ready;
    public event EventHandler<ErrorMessage>? Errored;
    public event EventHandler? Disconnected;
    public event EventHandler? Loading;
    public event EventHandler<PresenceMessage>? PresenceUpdated;

    public void ClearPresence()
    {
        _client?.ClearPresence();
    }

    private async Task<bool> CheckPresence(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || !text.ToLower().Contains("discord.gg"))
        {
            return true;
        }

        if (_disableSettings.InviteWarn)
        {
            return true;
        }
        var result = await MessageBox.Show(
            Language.GetText(LanguageText.AdvertisingWarning), 
            Language.GetText(LanguageText.Warning),
            MessageBoxButton.OkCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Ok)
        {
            _disableSettings.InviteWarn = true;

            _logger.Warning(Language.GetText(LanguageText.AdvertisingWarningDisabled));
            return true;
        }

        return result != MessageBoxResult.Cancel;
    }

    public void Start(long? applicationId, string? applicationName)
    {
        //TODO: Check if discord is running under admin and if we need to restart under admin (But warn about having both apps open with admin in the first place)
        
        _presenceId = applicationId ?? RpcPageManager.CurrentPage?.RichPresence.Id ?? Constants.MultiRPCID;
        var idS = _presenceId.ToString();

        var name = applicationName ?? RpcPageManager.CurrentPage?.RichPresence.Name ?? Language.GetText(LanguageText.MultiRPC);
        _presenceName = name;
            
        //If we are running already then stop it if we aren't using the same ID
        if (IsRunning && _client?.ApplicationID != idS)
        {
            Stop();
        }
        _logger.Information(Language.GetText(LanguageText.StartingRpc));

        var processName = SettingManager<GeneralSettings>.Setting.Client switch
        {
            DiscordClients.Discord => "Discord",
            DiscordClients.DiscordPTB => "DiscordPTB",
            DiscordClients.DiscordCanary => "DiscordCanary",
            DiscordClients.DiscordDevelopment => "DiscordDevelopment",
            _ => null
        };
        _client?.Dispose();

        var pipe = PipeUtil.FindPipe(processName);

        if (LoggingCreator.ShouldProcess(_logger.LogLevel, LogLevel.Trace))
        {
            var debugMessage = Language.GetText(LanguageText.DiscordClientCheck)
                .Replace("{discordProcess}", "{0}")
                .Replace("{wasFound}", "{1}");
            _logger.Debug(debugMessage, (Language.GetText(processName ?? "NA")), (pipe != -1).ToString());
        }
        _client = new DiscordRpcClient(idS, pipe)
        {
            SkipIdenticalPresence = false, 
            Logger = new RpcLogger(),
            ShutdownOnly = true
        };

        _client.OnPresenceUpdate += (sender, e) => PresenceUpdated?.Invoke(sender, e);
        _client.OnReady += (sender, e) => 
        {
            Status = ConnectionStatus.Connected;
            Ready?.Invoke(sender, e);
        };
        _client.OnError += (sender, e) =>
        {
            Status = ConnectionStatus.Connecting;
            Errored?.Invoke(this, e);
        };
        _rpcStart = DateTime.UtcNow;
        _client.Initialize();

        Status = ConnectionStatus.Connecting;
        Loading?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        if (_client?.IsDisposed ?? true)
        {
            return;
        }
        
        _logger.Information(Language.GetText(LanguageText.ShuttingDown));
        ClearPresence();
        _client?.Deinitialize();
        _client?.Dispose();
        Status = ConnectionStatus.Disconnected;
        _presenceId = 0;
        _presenceName = Language.GetText(LanguageText.Unknown);

        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public async Task UpdatePresence(DiscordRPC.RichPresence richPresence)
    {
        if (_client?.IsDisposed ?? true)
        {
            return;
        }
        
        //Check that the presence isn't doing any advertising
        if (!await CheckPresence(richPresence.Details) 
            || !await CheckPresence(richPresence.State))
        {
            Stop();
            return;
        }

        _client?.SetPresence(richPresence);
        
        if (richPresence.HasButtons() && !_disableSettings.ButtonWarn)
        {
            await MessageBox.Show(Language.GetText(LanguageText.ButtonWarn), Language.GetText(LanguageText.MultiRPC), MessageBoxButton.Ok, MessageBoxImage.Information);
            _disableSettings.ButtonWarn = true;
        }
    }

    public async Task UpdatePresence(Presence? richPresence)
    {
        if (richPresence == null)
        {
            return;
        }
            
        var pre = richPresence.RichPresence;
        pre.Buttons = pre.Buttons?
            .Where(x => !string.IsNullOrWhiteSpace(x.Url) && !string.IsNullOrWhiteSpace(x.Label))
            .ToArray();

        pre.Timestamps = richPresence.Profile.ShowTime ? new Timestamps
        {
            Start = _rpcStart
        } : null;
            
        if (richPresence.Id != _presenceId)
        {
            Stop();
            Start(richPresence.Id, richPresence.Name);
        }
        await UpdatePresence(pre);
    }
}