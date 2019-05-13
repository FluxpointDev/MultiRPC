using System;
using System.IO;
using MultiRPC.Functions;
using System.Diagnostics;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using static MultiRPC.JsonClasses.FileLocations;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for CreditsPage.xaml
    /// </summary>
    public partial class CreditsPage : Page
    {
        private CreditsList creditsList;
        private int retryCount; //Local retryCount

        public CreditsPage()
        {
            InitializeComponent();
            Loaded += CreditsPage_Loaded;
            SetupLogic();
        }

        private void CreditsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateText();
        }

        private Task UpdateText()
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
            FileInfo creditsFileFI = new FileInfo(CreditsFileLocalLocation);
            if (!updateText)
            {
                using (StreamReader reader = creditsFileFI.OpenText())
                {
                    creditsList = (CreditsList) App.JsonSerializer.Deserialize(reader, typeof(CreditsList));
                    if (creditsList != null)
                    {
                        tblCommunityAdmins.Text = string.Join("\r\n\r\n", creditsList.Admins);
                        tblPatreonDonators.Text = string.Join("\r\n\r\n", creditsList.Patreon);
                        tblPaypalDonators.Text = string.Join("\r\n\r\n", creditsList.Paypal);
                    }
                }
            }

            tblLastUpdated.Text = creditsFileFI.LastWriteTime.Date == DateTime.Now.Date ? 
                $"{App.Text.LastUpdatedAt}: {creditsFileFI.LastWriteTime.ToShortTimeString()}" : 
                $"{App.Text.LastUpdatedOn}: {creditsFileFI.LastWriteTime.ToLongDateString()}";
            return Task.CompletedTask;
        }

        private async Task SetupLogic()
        {
            if (File.Exists(CreditsFileLocalLocation))
                UpdateCredits();

            App.WebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            var webFile = System.Extra.Uri.Combine(App.MuiltiRPCWebsiteRoot, CreditsFileName);

            DownloadFile:
            try
            {
                if (!Utils.NetworkIsAvailable())
                {
                    int second = 5;
                    while (second != 0)
                    {
                        tblLastUpdated.Text = $"{App.Text.WaitingForInternetUpdate.Replace("{second}", second.ToString())}...";
                        await Task.Delay(1000);
                        second--;
                    }
                    goto DownloadFile;
                }
                await App.WebClient.DownloadFileTaskAsync(webFile, CreditsFileLocalLocation + ".new");
            }
            catch (Exception e)
            {
                App.Logging.Application(e.Message);

                if (File.Exists(CreditsFileLocalLocation + ".new"))
                    File.Delete(CreditsFileLocalLocation + ".new");

                if (retryCount == App.RetryCount)
                {
                    App.WebClient.DownloadFileCompleted -= WebClient_DownloadFileCompleted;
                    await UpdateCredits();
                }
                else
                {
                    retryCount++;
                    goto DownloadFile;
                }
            }
        }

        private void LinkUri_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.OriginalString);
            e.Handled = true;
        }

        private async void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            bool doLogic;
            try
            {
                var uri = (Uri) ((TaskCompletionSource<object>) e.UserState).Task.AsyncState;
                doLogic = uri.AbsoluteUri == System.Extra.Uri.Combine(App.MuiltiRPCWebsiteRoot, CreditsFileName);
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
            App.WebClient.DownloadFileCompleted -= WebClient_DownloadFileCompleted;
            await UpdateCredits();
        }
    }
}
