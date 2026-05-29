using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Extensions;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Overlays;

public partial class DiscordCheckOverlay : Panel
{
    private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(DiscordCheckOverlay));
    private bool _ranFadeOut;
    public DiscordCheckOverlay()
    {
        if (SettingManager<DisableSettings>.Setting.DiscordCheck)
        {
            IsVisible = false;
            return;
        }

        InitializeComponent();
        tblMadeBy.Text = Language.GetText(LanguageText.MadeBy) + ": " + Constants.AppDeveloper;
        rDiscordServer.Text = Language.GetText(LanguageText.DiscordServer) + ": ";
        hylServerLinkUri.Text = Constants.ServerInviteCode;
        hylServerLinkUri.Uri = Constants.DiscordServerUrl;
        btnDisableDiscordCheck.DataContext = (Language)LanguageText.TempDisableDiscordCheck;
        imgIcon.AddSvgAsset("Logo.svg");
        gifLoading.SourceStream = AssetManager.GetSeekableStream("Loading.gif");
        AssetManager.RegisterForAssetReload("Loading.gif",
            () =>
            {
                if (IsVisible)
                {
                    gifLoading.SourceStream = AssetManager.GetSeekableStream("Loading.gif");
                }
            });

        _ = WaitForDiscord();
        _ = ShowTmpButton();
    }

    private async Task ShowTmpButton()
    {
        await Task.Delay(5000);
        btnDisableDiscordCheck.Opacity = 1;
    }

    private bool GetClient(string requestedClient, out string client)
    {
        var haveClient = Process.GetProcessesByName(requestedClient).Length != 0;
        client = haveClient ? requestedClient : "";
        return haveClient;
    }
        
    private async Task WaitForDiscord()
    {
        try
        {
            //Lets first try to find what discord is open
            string discordClient = "";
            while (!_ranFadeOut)
            {
                if (GetClient("Discord", out discordClient))
                {
                    break;
                }
                if (GetClient("DiscordCanary", out discordClient))
                {
                    break;
                }
                if (GetClient("DiscordPTB", out discordClient))
                {
                    break;
                }
                if (GetClient("DiscordDevelopment", out discordClient))
                {
                    break;
                }
                    
                tblDiscordClientMessage.Text = Language.GetText(LanguageText.CantFindDiscord);
                await Task.Delay(750);
            }

            if (!_ranFadeOut)
            {
                tblMultiRPC.Text = "MultiRPC - " + Language.GetText(discordClient);
            }

            var processExpectedCount = 1;
            if (OperatingSystem.IsLinux())
            {
                processExpectedCount = 2;
            }
            else if (OperatingSystem.IsWindows())
            {
                processExpectedCount = 4;
            }
            while (!_ranFadeOut)
            {
                //If we have less then processExpectedCount from discord then discord itself is still loading
                var processCount = Process.GetProcessesByName(discordClient).Length;
                if (processCount < processExpectedCount)
                {
                    tblDiscordClientMessage.Text =
                        $"{Language.GetText(discordClient)} {Language.GetText(LanguageText.IsLoading)}....";
                    await Task.Delay(750);
                    continue;
                }

                _ = FadeOut();
            }
        }
        catch
        {
            _logger.Error(Language.GetText(LanguageText.ProcessFindError));
            _ = FadeOut();
        }
    }

    private async Task FadeOut()
    {
        _ranFadeOut = true;
        Opacity = 0;
        await Task.Delay(500);
        IsVisible = false;
        gifLoading.StopAndDispose();
    }

    private void BtnDisableDiscordCheck_OnClick(object? sender, RoutedEventArgs e) => _ = FadeOut();
}