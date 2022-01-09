using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MultiRPC.Discord;
using MultiRPC.Extensions;
using MultiRPC.UI.Controls;
using MultiRPC.Utils;
using TinyUpdate.Core.Extensions;
using TinyUpdate.Core.Helper;

namespace MultiRPC.UI.Pages.Settings;

public partial class AboutSettingsTab : UserControl, ITabPage
{
    public AboutSettingsTab()
    {
        InitializeComponent();
    }

    public Language? TabName { get; } = Language.GetLanguage(LanguageText.About);
    public bool IsDefaultPage => true;
    public void Initialize(bool loadXaml)
    {
        //TODO: Remove Windows check when we add cross platform support for gaining admin mode
        btnAdmin.IsEnabled = !AdminUtil.IsAdmin && OSHelper.ActiveOS == OSPlatform.Windows;

        tblName.Text += Assembly.GetEntryAssembly().GetSemanticVersion();
        var madeByLang = Language.GetLanguage(LanguageText.MadeBy);
        madeByLang.TextObservable.Subscribe(x => tblMadeBy.Text = x + ": " + Constants.AppDeveloper);
        tblDiscord.DataContext = Language.GetLanguage(LanguageText.Discord);
        tblDonations.DataContext = Language.GetLanguage(LanguageText.Donations);
        btnDonate.DataContext = Language.GetLanguage(LanguageText.ClickToDonate);
        tblDonationInfo.DataContext = Language.GetLanguage(LanguageText.DonateMessage);
        btnAdmin.DataContext = Language.GetLanguage(LanguageText.Admin);
        btnChangelog.DataContext = Language.GetLanguage(LanguageText.Changelog);
        btnCheckUpdate.DataContext = Language.GetLanguage(LanguageText.CheckForUpdates);
        _ = CheckDiscordStatus();

        var githubTooltipLang = Language.GetLanguage(LanguageText.GithubTooltip);
        githubTooltipLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(imgGithub, x));
            
        var fluxpointTooltipLang = Language.GetLanguage(LanguageText.FluxpointTooltip);
        fluxpointTooltipLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(imgFluxpoint, x));
 
        var discordTooltipLang = Language.GetLanguage(LanguageText.JoinForFunBotsAndSupport);
        discordTooltipLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(brdDiscord, x));
            
        imgIcon.AddSvgAsset("Logo.svg");
        imgGithub.AddSvgAsset("Icons/Github.svg");
        imgFluxpoint.AddSvgAsset("Icons/Fluxpoint.svg");
        imgDiscord.AddSvgAsset("Icons/Discord.svg");
    }

    private async Task CheckDiscordStatus()
    {
        brdDiscordStatus.Background = Brushes.Orange;
        var lang = new Language(LanguageText.CheckingDiscordStatus);
        tblDiscordStatus.DataContext = lang;
        //TODO: Put this on another thread so it doesn't make the UI un-responsive
        var status = await DiscordStatusChecker.GetStatus();
        switch (status)
        {
            case DiscordStatus.Operational:
                brdDiscordStatus.Background = Brushes.Green;
                lang.ChangeJsonNames(LanguageText.Operational);
                break;
            case DiscordStatus.PartialOutage:
                brdDiscordStatus.Background = Brushes.OrangeRed;
                lang.ChangeJsonNames(LanguageText.PartialOutage);
                break;
            case DiscordStatus.Degraded:
                brdDiscordStatus.Background = Brushes.OrangeRed;
                lang.ChangeJsonNames(LanguageText.DegradedPerformance);
                break;
            case DiscordStatus.MajorOutage:
                brdDiscordStatus.Background = Brushes.Red;
                lang.ChangeJsonNames(LanguageText.MajorOutage);
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
        if (await MessageBox.Show(
                Language.GetText(LanguageText.AdminWarning), 
                Language.GetText(LanguageText.MultiRPC), 
                MessageBoxButton.OkCancel, 
                MessageBoxImage.Warning) != MessageBoxResult.Ok)
        {
            return;
        }
            
        try
        {
            var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe")) //Net Core tell's us the location of the dll, not the exe so we point it back to the exe
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
        await MessageBox.Show(Language.GetText(LanguageText.ToAdd));
    }

    private async void BtnCheckUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        await MessageBox.Show(Language.GetText(LanguageText.ToAdd));
    }

    private void ImgGithub_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        "https://github.com/FluxpointDev/MultiRPC".OpenInBrowser();
    }

    private void ImgFluxpoint_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Constants.WebsiteUrl.OpenInBrowser();
    }

    private async void ImgDiscord_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        await MessageBox.Show(Language.GetText(LanguageText.JoinServerMessage),
            Language.GetText(LanguageText.DiscordServer),
            MessageBoxButton.Ok,
            MessageBoxImage.Information);
        Constants.DiscordServerUrl.OpenInBrowser();
    }
}