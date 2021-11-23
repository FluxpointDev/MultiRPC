using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MultiRPC.Discord;
using MultiRPC.Extensions;
using MultiRPC.UI.Controls;
using TinyUpdate.Core.Extensions;

namespace MultiRPC.UI.Pages.Settings
{
    public partial class AboutSettingsTab : UserControl, ITabPage
    {
        public AboutSettingsTab()
        {
            InitializeComponent();
        }

        public Language? TabName { get; } = new Language("About");
        public bool IsDefaultPage => true;
        public void Initialize(bool loadXaml)
        {
            tblName.Text += Assembly.GetEntryAssembly().GetSemanticVersion();
            var madeByLang = new Language("MadeBy");
            madeByLang.TextObservable.Subscribe(x => tblMadeBy.Text = x + ": " + Constants.AppDeveloper);
            tblDiscord.DataContext = new Language("Discord");
            tblDonations.DataContext = new Language("Donations");
            btnDonate.DataContext = new Language("ClickToDonate");
            tblDonationInfo.DataContext = new Language("DonateMessage");
            btnAdmin.DataContext = new Language("Admin");
            btnChangelog.DataContext = new Language("Changelog");
            btnCheckUpdate.DataContext = new Language("CheckForUpdates");
            _ = CheckDiscordStatus();

            var githubTooltipLang = new Language("GithubTooltip");
            githubTooltipLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgGithub, x));
            
            var fluxpointTooltipLang = new Language("FluxpointTooltip");
            fluxpointTooltipLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgFluxpoint, x));
 
            var discordTooltipLang = new Language("JoinForFunBotsAndSupport");
            discordTooltipLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgDiscord, x));
        }

        private async Task CheckDiscordStatus()
        {
            brdDiscordStatus.Background = Brushes.Orange;
            var lang = new Language("CheckingDiscordStatus");
            tblDiscordStatus.DataContext = lang;
            var status = await DiscordStatusChecker.GetStatus();
            switch (status)
            {
                case DiscordStatus.Operational:
                    brdDiscordStatus.Background = Brushes.Green;
                    lang.ChangeJsonNames("Operational");
                    break;
                case DiscordStatus.PartialOutage:
                    brdDiscordStatus.Background = Brushes.OrangeRed;
                    lang.ChangeJsonNames("PartialOutage");
                    break;
                case DiscordStatus.Degraded:
                    brdDiscordStatus.Background = Brushes.OrangeRed;
                    lang.ChangeJsonNames("DegradedPerformance");
                    break;
                case DiscordStatus.MajorOutage:
                    brdDiscordStatus.Background = Brushes.Red;
                    lang.ChangeJsonNames("MajorOutage");
                    break;
            }
        }

        private void BtnDonate_OnClick(object? sender, RoutedEventArgs e)
        {
            "https://fluxpoint.dev/donate".OpenInBrowser();
        }

        //TODO: Make cross platform
        private async void BtnAdmin_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase.Replace(".dll", ".exe")) //Net Core tell's us the location of the dll, not the exe so we point it back to the exe
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(processInfo);
                ((App)Application.Current).DesktopLifetime!.Shutdown();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode != 1223)
                {
                    await MessageBox.Show(ex.Message);
                    //TODO: Log
                    // Something happened
                }
            }
        }

        private async void BtnChangelog_OnClick(object? sender, RoutedEventArgs e)
        {
            await MessageBox.Show("TO ADD");
        }

        private async void BtnCheckUpdate_OnClick(object? sender, RoutedEventArgs e)
        {
            await MessageBox.Show("TO ADD");
        }

        private void ImgGithub_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            "https://github.com/FluxpointDev/MultiRPC".OpenInBrowser();
        }

        private void ImgFluxpoint_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Constants.WebsiteUrl.OpenInBrowser();
        }

        private const string DiscordServerUrl = "https://discord.gg/" + Constants.ServerInviteCode;
        private async void ImgDiscord_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            await MessageBox.Show(Language.GetText("JoinServerMessage"),
                Language.GetText("DiscordServer"),
                MessageBoxButton.Ok,
                MessageBoxImage.Information);
            DiscordServerUrl.OpenInBrowser();
        }
    }
}