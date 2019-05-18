using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using static MultiRPC.JsonClasses.FileLocations;
using Uri = System.Extra.Uri;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for CreditsPage.xaml
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

            tblLastUpdated.Text = creditsFileFI.LastWriteTime.Date == DateTime.Now.Date
                ? $"{App.Text.LastUpdatedAt}: {creditsFileFI.LastWriteTime.ToShortTimeString()}"
                : $"{App.Text.LastUpdatedOn}: {creditsFileFI.LastWriteTime.ToLongDateString()}";

            return Task.CompletedTask;
        }

        private async Task SetupLogic()
        {
            if (File.Exists(CreditsFileLocalLocation))
                UpdateCredits();

            _webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            var webFile = Uri.Combine(App.MultiRPCWebsiteRoot, CreditsFileName);

            DownloadFile:
            try
            {
                if (!Utils.NetworkIsAvailable())
                {
                    var second = 5;
                    while (second != 0)
                    {
                        tblLastUpdated.Text =
                            $"{App.Text.WaitingForInternetUpdate.Replace("{second}", second.ToString())}...";
                        await Task.Delay(1000);
                        second--;
                    }

                    goto DownloadFile;
                }

                await _webClient.DownloadFileTaskAsync(webFile, CreditsFileLocalLocation + ".new");
            }
            catch (Exception e)
            {
                App.Logging.Application(e.Message);

                if (File.Exists(CreditsFileLocalLocation + ".new"))
                    File.Delete(CreditsFileLocalLocation + ".new");

                if (_retryCount == App.RetryCount)
                {
                    _webClient.DownloadFileCompleted -= WebClient_DownloadFileCompleted;
                    await UpdateCredits();
                }
                else
                {
                    _retryCount++;
                    goto DownloadFile;
                }
            }
        }

        private void LinkUri_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.OriginalString);
            e.Handled = true;
        }

        private async void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bool doLogic;
            try
            {
                var uri = (System.Uri) ((TaskCompletionSource<object>) e.UserState).Task.AsyncState;
                doLogic = uri.AbsoluteUri == Uri.Combine(App.MultiRPCWebsiteRoot, CreditsFileName);
            }
            catch (Exception ex)
            {
                App.Logging.Application(ex.Message);
                return;
            }

            if (!doLogic) return;
            if (File.Exists(CreditsFileLocalLocation))
                File.Delete(CreditsFileLocalLocation);

            File.Move(CreditsFileLocalLocation + ".new", CreditsFileLocalLocation);
            _webClient.DownloadFileCompleted -= WebClient_DownloadFileCompleted;
            await UpdateCredits();
        }
    }
}