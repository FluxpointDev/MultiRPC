using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Overlays
{
    public partial class DiscordCheckOverlay : UserControl
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
            tblMadeBy.Text = Language.GetText("MadeBy") + ": " + Constants.AppDeveloper;
            rDiscordServer.Text = Language.GetText("DiscordServer") + ": ";
            hylServerLinkUri.Text = Language.GetText("ServerInviteCode");
            hylServerLinkUri.Uri = "https://discord.gg/" + Constants.ServerInviteCode;

            _ = WaitForDiscord();
            _ = ShowTmpButton();
        }

        //TODO: Add imgLoading
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
                    
                    tblDiscordClientMessage.Text = Language.GetText("CantFindDiscord");
                    await Task.Delay(750);
                }

                if (!_ranFadeOut)
                {
                    tblMultiRPC.Text = "MultiRPC - " + Language.GetText(discordClient);
                }

                while (!_ranFadeOut)
                {
                    //If we have less then 4 processes from discord then discord itself is still loading
                    var processCount = Process.GetProcessesByName(discordClient).Length;
                    if (processCount < 4)
                    {
                        tblDiscordClientMessage.Text =
                            $"{Language.GetText(discordClient)} {Language.GetText("IsLoading")}....";
                        await Task.Delay(750);
                        continue;
                    }

                    _ = FadeOut();
                }
            }
            catch
            {
                _logger.Error(Language.GetText("ProcessFindError"));
                _ = FadeOut();
            }
        }

        private async Task FadeOut()
        {
            //TODO: Add animation
            _ranFadeOut = true;
            Opacity = 0;
            //await Task.Delay(500);
            IsVisible = false;
        }

        private void BtnDisableDiscordCheck_OnClick(object? sender, RoutedEventArgs e) => _ = FadeOut();
    }
}