using System;
using System.IO;
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
        private CreditsList CreditsList;
        private int retryCount;

        public CreditsPage()
        {
            InitializeComponent();
            Loaded += CreditsPage_Loaded;
            if (File.Exists(CreditsFileLocalLocation))
                UpdateCredits();
            SetupLogic();
        }

        private void CreditsPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateText();
        }

        public async Task UpdateText()
        {
            tblCommunityAdminsTitle.Text = App.Text.CommunityAdmins;
            tblPatreonDonatorsTitle.Text = App.Text.PatreonDonators;
            tblPaypalDonatorsTitle.Text = App.Text.PaypalDonators;
            tblIconProvidersTitle.Text = App.Text.IconProviders;
            await UpdateCredits(true);
        }

        public async Task UpdateCredits(bool updateText = false)
        {
            FileInfo CreditsFileFI = new FileInfo(CreditsFileLocalLocation);
            if (!updateText)
                using (StreamReader reader = CreditsFileFI.OpenText())
                {
                    CreditsList = (CreditsList) App.JsonSerializer.Deserialize(reader, typeof(CreditsList));
                    if (CreditsList != null)
                    {
                        tblCommunityAdmins.Text = string.Join("\r\n\r\n", CreditsList.Admins);
                        tblPatreonDonators.Text = string.Join("\r\n\r\n", CreditsList.Patreon);
                        tblPaypalDonators.Text = string.Join("\r\n\r\n", CreditsList.Paypal);
                    }
                }

            if (CreditsFileFI.LastWriteTime.Date == DateTime.Now.Date)
                tblLastUpdated.Text = $"{App.Text.LastUpdatedAt}: {CreditsFileFI.LastWriteTime.ToShortTimeString()}";
            else
                tblLastUpdated.Text = $"{App.Text.LastUpdatedOn}: {CreditsFileFI.LastWriteTime.ToLongDateString()}";
        }

        public async Task SetupLogic()
        {
            await UpdateText();
            App.WebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            var webFile = System.Extra.Uri.Combine(App.MuiltiRPCWebsiteRoot, CreditsFileName);
            tblLastUpdated.Text = $"{App.Text.Loading}...";
            DownloadFile:
            try
            {
                await App.WebClient.DownloadFileTaskAsync(webFile, CreditsFileLocalLocation + ".new");
            }
            catch
            {
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
            bool DoLogic = false;
            try
            {
                var uri = (Uri)((TaskCompletionSource<object>) e.UserState).Task.AsyncState;
                DoLogic = uri.AbsoluteUri == System.Extra.Uri.Combine(App.MuiltiRPCWebsiteRoot, CreditsFileName);
            }
            catch { }

            if (DoLogic)
            {
                if (File.Exists(CreditsFileLocalLocation))
                    File.Delete(CreditsFileLocalLocation);

                File.Move(CreditsFileLocalLocation + ".new", CreditsFileLocalLocation);
                App.WebClient.DownloadFileCompleted -= WebClient_DownloadFileCompleted;
                await UpdateCredits();
            }
        }
    }
}
