using System.Windows.Navigation;
using MultiRPC.Core.Enums;
using MultiRPC.Core;
using System.IO;
using System;
using System.Net.NetworkInformation;
using System.Net.Http;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Notification;

namespace MultiRPC.GUI.CorePages
{
    /// <summary>
    /// Interaction logic for CreditsPage.xaml
    /// </summary>
    public partial class CreditsPage : PageWithIcon
    {
        public override MultiRPCIcons IconName { get; } = MultiRPCIcons.Credits;
        public override string JsonContent => "Credits";

        private int RetryCount;
        private CreditsList CreditsList = null;
        FileInfo CreditsFile = new FileInfo(FileLocations.CreditsFileLocation);
        private HttpClient HttpClient = new HttpClient 
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public CreditsPage()
        {
            InitializeComponent();

            Settings.Current.LanguageChanged += (_, __) => Dispatcher.Invoke(() => UpdateText());
            NetworkChange.NetworkAvailabilityChanged += (_,__) => Dispatcher.Invoke(() => UpdateCredits());
            Loaded += (_,__) => Dispatcher.Invoke(() => UpdateCredits());

            UpdateText();
            UpdateCreditsUI();
        }

        private async void UpdateCredits() 
        {
            var webFile = $"{Constants.MultiRPCWebsiteRoot}/{FileLocations.CreditsFileName}";

            while (Constants.RetryCount > RetryCount)
            {
                try
                {
                    if (!Utils.NetworkIsAvailable())
                    {
                        tblLastUpdated.Text =
                            $"{LanguagePicker.GetLineFromLanguageFile("WaitingForInternetUpdate")}...";
                        return;
                    }

                    tblLastUpdated.Text = LanguagePicker.GetLineFromLanguageFile("CheckForUpdates").Replace("\r\n", " ");
                    HttpClient.CancelPendingRequests();

                    var creditFileContent = await HttpClient.GetStringAsync(webFile);
                    if (!string.IsNullOrWhiteSpace(creditFileContent))
                    {
                        File.WriteAllText(FileLocations.CreditsFileLocation, creditFileContent);
                        UpdateCreditsUI();
                        UpdateText();
                        return;
                    }
                }
                catch (Exception e)
                {
                    NotificationCenter.Logger.Error(e.Message);
                }
            }
            RetryCount = 0;
        }

        private void UpdateCreditsUI()
        {
            if (!CreditsFile.Exists) 
            {
                return;
            }

            using var reader = CreditsFile.OpenText();
            CreditsList = (CreditsList)Constants.JsonSerializer.Deserialize(reader, typeof(CreditsList));
            if (CreditsList != null)
            {
                tblCommunityAdmins.Text = string.Join("\r\n\r\n", CreditsList.Admins);
                tblPatreonDonators.Text = string.Join("\r\n\r\n", CreditsList.Patreon);
                tblPaypalDonators.Text = string.Join("\r\n\r\n", CreditsList.Paypal);
            }
        }

        private void UpdateText() 
        {
            tblCommunityAdminsTitle.Text = LanguagePicker.GetLineFromLanguageFile("CommunityAdmins");
            tblPatreonDonatorsTitle.Text = LanguagePicker.GetLineFromLanguageFile("PatreonDonators");
            tblPaypalDonatorsTitle.Text = LanguagePicker.GetLineFromLanguageFile("PaypalDonators");
            tblIconProvidersTitle.Text = LanguagePicker.GetLineFromLanguageFile("IconProviders");

            if (CreditsFile.Exists)
            {
                tblLastUpdated.Text = CreditsFile.LastWriteTime.Date == DateTime.Now.Date
                ? $"{LanguagePicker.GetLineFromLanguageFile("LastUpdatedAt")}: {CreditsFile.LastWriteTime.ToShortTimeString()}"
                : $"{LanguagePicker.GetLineFromLanguageFile("LastUpdatedOn")}: {CreditsFile.LastWriteTime.ToLongDateString()}";
            }
        }

        private void LinkUri_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Uri.OpenWebsite();
            e.Handled = true;
        }
    }
}
