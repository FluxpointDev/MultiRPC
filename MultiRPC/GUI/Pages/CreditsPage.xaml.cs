using System;
using System.Diagnostics;
using System.Extra;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using static MultiRPC.JsonClasses.FileLocations;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for CreditsPage.xaml
    /// </summary>
    public partial class CreditsPage : Page
    {
        public static CreditsPage _CreditsPage;
        private readonly WebClient _webClient = new WebClient();
        private CreditsList _creditsList;
        private int _retryCount; //Local retryCount

        public CreditsPage()
        {
            InitializeComponent();

            _CreditsPage = this;
            UpdateText();
            SetupLogic();
        }

        public Task UpdateText()
        {
            tblCommunityAdminsTitle.Text = App.Text.CommunityAdmins;
            tblPatreonDonatorsTitle.Text = App.Text.PatreonDonators;
            tblPaypalDonatorsTitle.Text = App.Text.PaypalDonators;
            tblIconProvidersTitle.Text = App.Text.IconProviders;
            UpdateCredits(true);

            return Task.CompletedTask;
        }

        private Task UpdateCredits(bool updateText = false)
        {
            var creditsFileFI = new FileInfo(CreditsFileLocalLocation);
            if (!updateText)
            {
                using (var reader = creditsFileFI.OpenText())
                {
                    _creditsList = (CreditsList) App.JsonSerializer.Deserialize(reader, typeof(CreditsList));
                    if (_creditsList != null)
                    {
                        tblCommunityAdmins.Text = string.Join("\r\n\r\n", _creditsList.Admins);
                        tblPatreonDonators.Text = string.Join("\r\n\r\n", _creditsList.Patreon);
                        tblPaypalDonators.Text = string.Join("\r\n\r\n", _creditsList.Paypal);
                    }
                }
            }

            tblLastUpdated.Text = creditsFileFI.LastWriteTime.Date == DateTime.Now.Date
                ? $"{App.Text.LastUpdatedAt}: {creditsFileFI.LastWriteTime.ToShortTimeString()}"
                : $"{App.Text.LastUpdatedOn}: {creditsFileFI.LastWriteTime.ToLongDateString()}";

            return Task.CompletedTask;
        }

        private async Task SetupLogic()
        {
            if (File.Exists(CreditsFileLocalLocation))
            {
                await UpdateCredits();
            }

            var webFile = new[] {App.MultiRPCWebsiteRoot, CreditsFileName}.Combine();
            DownloadFile:
            try
            {
                if (!Utils.NetworkIsAvailable())
                {
                    NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
                    tblLastUpdated.Text =
                        $"{App.Text.WaitingForInternetUpdate}...";
                    return;
                }

                tblLastUpdated.Text = App.Text.CheckForUpdates.Replace("\r\n", " ");
                if (_webClient.IsBusy)
                {
                    _webClient.CancelAsync();
                }

                var creditFileContent = await _webClient.DownloadStringTaskAsync(webFile);
                if (!string.IsNullOrWhiteSpace(creditFileContent))
                {
                    File.WriteAllText(CreditsFileLocalLocation, creditFileContent);
                    await UpdateCredits();
                }
                else
                {
                    if (_retryCount == App.RetryCount)
                    {
                        await UpdateCredits();
                    }
                    else
                    {
                        _retryCount++;
                        goto DownloadFile;
                    }
                }
            }
            catch (Exception e)
            {
                App.Logging.Application(e.Message);

                if (_retryCount == App.RetryCount)
                {
                    await UpdateCredits();
                }
                else
                {
                    _retryCount++;
                    goto DownloadFile;
                }
            }
        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (Utils.NetworkIsAvailable())
            {
                Dispatcher.Invoke(async () => await SetupLogic());
            }
        }

        private void LinkUri_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Uri.OpenWebsite();
            e.Handled = true;
        }
    }
}