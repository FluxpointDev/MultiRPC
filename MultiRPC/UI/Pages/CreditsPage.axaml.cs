using System;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;
using MultiRPC.Extensions;
using MultiRPC.Utils;
using TinyUpdate.Core.Logging;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.UI.Pages;

public partial class CreditsPage : SidePage
{
    public override string IconLocation => "Icons/Credits";
    public override string LocalizableName => "Credits";
    public override void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        tblCommunityAdminsTitle.DataContext = Language.GetLanguage(LanguageText.CommunityAdmins);
        tblPatreonDonatorsTitle.DataContext = Language.GetLanguage(LanguageText.PatreonDonators);
        tblPaypalDonatorsTitle.DataContext = Language.GetLanguage(LanguageText.PaypalDonators);
        tblIconProvidersTitle.DataContext = Language.GetLanguage(LanguageText.IconProviders);
        NetworkChange.NetworkAddressChanged += NetworkChangeOnNetworkAddressChanged;
        UpdateCredits();
        _ = DownloadAndShow();
            
        imgCommunityAdmins.AddSvgAsset("Icons/Shield.svg");
        imgPatreonDonators.AddSvgAsset("Icons/Heart.svg");
        imgPaypalDonators.AddSvgAsset("Icons/Heart.svg");
        imgIconProviders.AddSvgAsset("Icons/Heart.svg");
        Language.LanguageChanged += OnLanguageChanged;
    }

    private async void NetworkChangeOnNetworkAddressChanged(object? sender, EventArgs e)
    {
        if (NetworkUtil.NetworkIsAvailable())
        {
            await DownloadAndShow();
        }
    }

    private static readonly string CreditsFileLocation = Path.Combine(Constants.SettingsFolder, "Credits.json");
    private CreditsList? _creditsList;
    private bool _downloadedCredit;

    private async Task DownloadAndShow()
    {
        await DownloadCredits();
        this.RunUILogic(UpdateCredits);
    }
        
    private void UpdateCredits()
    {
        var creditsFileInfo = new FileInfo(CreditsFileLocation);
        if (!creditsFileInfo.Exists)
        {
            return;
        }
        _writeTime = creditsFileInfo.LastWriteTime;

        using var reader = creditsFileInfo.OpenRead();
        _creditsList = JsonSerializer.Deserialize(reader, CreditsListContext.Default.CreditsList);
        if (_creditsList != null)
        {
            tblCommunityAdmins.Text = string.Join("\r\n\r\n", _creditsList.Admins);
            tblPatreonDonators.Text = string.Join("\r\n\r\n", _creditsList.Patreon);
            tblPaypalDonators.Text = string.Join("\r\n\r\n", _creditsList.Paypal);
        }

        UpdateLastUpdate();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        UpdateLastUpdate();
    }

    private void UpdateLastUpdate()
    {
        if (!NetworkUtil.NetworkIsAvailable() && !_downloadedCredit)
        {
            tblLastUpdated.Text = $"{Language.GetText(LanguageText.WaitingForInternetUpdate)}...";
            return;
        }
            
        tblLastUpdated.Text = _writeTime.Date == DateTime.Now.Date
            ? $"{Language.GetText(LanguageText.LastUpdatedAt)}: {_writeTime.ToShortTimeString()}"
            : $"{Language.GetText(LanguageText.LastUpdatedOn)}: {_writeTime.ToLongDateString()}";
    }

    private DateTime _writeTime = DateTime.MinValue;
    private const string Url = "https://multirpc.fluxpoint.dev/Credits.json";
    private readonly ILogging _logger = LoggingCreator.CreateLogger(nameof(CreditsPage));
    private async Task DownloadCredits()
    {
        if (_downloadedCredit)
        {
            return;
        }

        for (int i = 0; i < Constants.RetryCount; i++)
        {
            this.RunUILogic(() => 
                tblLastUpdated.Text = Language.GetText(LanguageText.CheckForUpdates).Replace("\r\n", " "));

            var req = await App.HttpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get, Url));
            if (req is null || !req.IsSuccessStatusCode)
            {
                if (req == null)
                {
                    _logger.Error("Credit request returned nothing");
                }
                else
                {
                    _logger.Error("StatusCode: {0}, Reason: {1}", req.StatusCode, req.ReasonPhrase);
                }
                continue;
            }

            var creditStream = await req.Content.ReadAsStreamAsync();
            if (creditStream.Length == 0)
            {
                _logger.Error("Credit stream contains nothing!");
                continue;
            }

            if (File.Exists(CreditsFileLocation))
            {
                File.Delete(CreditsFileLocation);
            }
            var fileStream = File.OpenWrite(CreditsFileLocation);
            await creditStream.CopyToAsync(fileStream);
            await creditStream.DisposeAsync();
            await fileStream.DisposeAsync();
            _downloadedCredit = true;
            break;
        }
    }
}