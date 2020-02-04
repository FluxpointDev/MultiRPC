using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiRPC.Core.Enums;
using MultiRPC.Core;
using MultiRPC.GUI.MessageBox;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;
using Itschwabing.Libraries.ResourceChangeEvent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.ComponentModel;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Notification;

namespace MultiRPC.GUI.CorePages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : PageWithIcon
    {
        public override MultiRPCIcons IconName { get; } = MultiRPCIcons.Settings;
        public override string JsonContent => "Settings";

        public FileWatch ChanglogFileWatch = new FileWatch(FileLocations.ChangelogFileName);

        public SettingsPage()
        {
            //ToDo: Cleanup

            InitializeComponent();
            UpdateText();
            Settings.Current.LanguageChanged += (_,__) => UpdateText();
            ChanglogFileWatch.Created += FileWatcher_Created;
            ChanglogFileWatch.Deleted += FileWatcher_Created;

            btnChangelog.IsEnabled = File.Exists(FileLocations.ChangelogFileLocation);
            imgDownloadRPC.Tag = Constants.MultiRPCWebsiteRoot;
            rAppDev.Text = Constants.AppDeveloper;

            btnAdmin.IsEnabled = !Utils.RunningAsAdministrator;
            cbDiscordCheck.IsChecked = !Settings.Current.DiscordCheck;
            cbTokenCheck.IsChecked = !Settings.Current.CheckToken;
            cbHelpIcons.IsChecked = !Settings.Current.ShowHelpIcons;
            cbAutoUpdating.IsChecked = !Settings.Current.AutoUpdate;
            cbHideTaskbarIcon.IsChecked = !Settings.Current.HideTaskbarIconWhenMin;
            cbShowPageTooltips.IsChecked = !Settings.Current.ShowNavigationTooltips;

            cbDiscordClient.SelectedIndex = (int)Settings.Current.ClientToUse;
            cbAfkTime.IsChecked = Settings.Current.AFKTime;

            ComboBoxItem item = null;
            var autoStartS = "";
            switch (Settings.Current.AutoStart)
            {
                case AutoStart.No:
                    autoStartS = LanguagePicker.GetLineFromLanguageFile("No");
                    break;
                case AutoStart.MultiRPC:
                    autoStartS = "MultiRPC";
                    break;
                case AutoStart.Custom:
                    autoStartS = LanguagePicker.GetLineFromLanguageFile("Custom");
                    break;
            }
            for (var i = 0; i < cbAutoStart.Items.Count; i++)
            {
                var startItem = (ComboBoxItem)cbAutoStart.Items[i];
                if (startItem.Content.ToString() == autoStartS)
                {
                    item = startItem;
                    break;
                }
            }

            if (item != null)
            {
                cbAutoStart.SelectedItem = item;
            }

            var activeLangInt = 0;

            var files = Directory.GetFiles(Constants.LanguageFolder);
            var cbilangs = new List<ComboBoxItem>(files.Length);
            for (var i = 0; i < files.Length; i++)
            {
                var languageContent = JObject.Parse(File.ReadAllText(files[i]));

                var cbilang = new ComboBoxItem
                {
                    Content = languageContent.Value<string>("LanguageName"),
                    Tag = Path.GetFileNameWithoutExtension(files[i])
                };
                cbilangs.Add(cbilang);

                if (languageContent.Value<string>("LanguageTag") == Settings.Current.ActiveLanguage)
                {
                    activeLangInt = i;
                }
            }

            cbLanguage.ItemsSource = cbilangs;
            cbLanguage.SelectedIndex = activeLangInt;
        }

        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => btnChangelog.IsEnabled = File.Exists(FileLocations.ChangelogFileLocation));
        }

        public void UpdateText()
        {
            tblWebsiteMultiRPC.Text = LanguagePicker.GetLineFromLanguageFile("Website");
            tblDownload.Text = LanguagePicker.GetLineFromLanguageFile("Download");
            tblDiscordServer.Text = LanguagePicker.GetLineFromLanguageFile("DiscordServer");
            tblJoinForFunBotsAndSupport.Text = LanguagePicker.GetLineFromLanguageFile("JoinForFunBotsAndSupport");
            tblWebsiteDiscord.Text = LanguagePicker.GetLineFromLanguageFile("Website");
            tblStatus.Text = LanguagePicker.GetLineFromLanguageFile("Status");
            tblSettings.Text = LanguagePicker.GetLineFromLanguageFile("Settings");
            tblDiscordClient.Text = LanguagePicker.GetLineFromLanguageFile("Client") + ":";
            tblAutoStart.Text = LanguagePicker.GetLineFromLanguageFile("AutoStart") + ":";
            tblAfkTime.Text = LanguagePicker.GetLineFromLanguageFile("AfkTime") + ":";
            cbiAuto.Content = LanguagePicker.GetLineFromLanguageFile("Auto");
            cbiNo.Content = LanguagePicker.GetLineFromLanguageFile("No");
            cbiCustom.Content = LanguagePicker.GetLineFromLanguageFile("Custom");
            tblDisable.Text = LanguagePicker.GetLineFromLanguageFile("Disable");
            tblDiscordCheck.Text = LanguagePicker.GetLineFromLanguageFile("DiscordCheck") + ":";
            tblTokenCheck.Text = LanguagePicker.GetLineFromLanguageFile("TokenCheck") + ":";
            tblHelpIcons.Text = LanguagePicker.GetLineFromLanguageFile("HelpIcons") + ":";
            tblAutoUpdating.Text = LanguagePicker.GetLineFromLanguageFile("AutomaticUpdates") + ":";
            tblPaypal.Text = LanguagePicker.GetLineFromLanguageFile("PaypalMin1");
            tblPatreon.Text = LanguagePicker.GetLineFromLanguageFile("PatreonMonthly");
            btnPaypal.Content = LanguagePicker.GetLineFromLanguageFile("ClickHere");
            btnPatreon.Content = LanguagePicker.GetLineFromLanguageFile("ClickHere");
            tblDonations.Text = LanguagePicker.GetLineFromLanguageFile("Donations");
            tblDonateMessage.Text = LanguagePicker.GetLineFromLanguageFile("DonateMessage");
            btnChangelog.Content = LanguagePicker.GetLineFromLanguageFile("Changelog");
            btnCheckUpdates.Content = LanguagePicker.GetLineFromLanguageFile("CheckForUpdates");
            btnDebug.Content = LanguagePicker.GetLineFromLanguageFile("Debug");
            rMadeBy.Text = LanguagePicker.GetLineFromLanguageFile("MadeBy") + ": ";
            tblLanguage.Text = LanguagePicker.GetLineFromLanguageFile("Language");
            rAppDev.ToolTip = new ToolTip(LanguagePicker.GetLineFromLanguageFile("ClickToCopy"));
            tblHideTaskbarIcon.Text = LanguagePicker.GetLineFromLanguageFile("HideTaskbarIcon");
            tblShowPageTooltips.Text = LanguagePicker.GetLineFromLanguageFile("ShowPageTooltips");
            btnAdmin.Content = LanguagePicker.GetLineFromLanguageFile("Admin");
        }

        private void Image_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            _ = image.DoubleAnimation(0.8, image.Opacity);
        }

        private void Image_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            _ = image.DoubleAnimation(0.6, image.Opacity);
        }

        private async void Image_OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            image.Tag.ToString().OpenWebsite();
            await image.DoubleAnimation(1, image.Opacity);
            _ = image.DoubleAnimation(0.8, image.Opacity);
        }

        private async void ImgJoinForFunBotsAndSupport_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await CustomMessageBox.Show(LanguagePicker.GetLineFromLanguageFile("JoinServerMessage"),
                LanguagePicker.GetLineFromLanguageFile("DiscordServer"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            $"https://discord.gg/{Constants.ServerInviteCode}".OpenWebsite();
        }

        private void CbDiscordClient_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Current.ClientToUse = (DiscordClient)cbDiscordClient.SelectedIndex;
        }

        private void CbAutoStart_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Current.AutoStart = (AutoStart)cbAutoStart.SelectedIndex;
        }

        private void CbLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Current.ActiveLanguage = (cbLanguage.SelectedItem as ComboBoxItem).Tag.ToString();
        }

        private void CbAfkTime_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.AFKTime = (sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private void CbDiscordCheck_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.DiscordCheck = !(sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private void CbTokenCheck_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.CheckToken = !(sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private void CbHelpIcons_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.ShowHelpIcons = !(sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private void CbAutoUpdating_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.AutoUpdate = !(sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private void CbHideTaskbarIcon_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.HideTaskbarIconWhenMin = !(sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private void CbShowPageTooltips_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Current.ShowNavigationTooltips = !(sender as CheckBox).IsChecked.GetValueOrDefault();
        }

        private async void BtnPaypal_OnClick(object sender, RoutedEventArgs e)
        {
            await CustomMessageBox.Show(LanguagePicker.GetLineFromLanguageFile("PaypalDonateMessage"));
        }

        private void BtnPatreon_OnClick(object sender, RoutedEventArgs e)
        {
            "https://www.patreon.com/builderb".OpenWebsite();
        }

        private void RAppDev_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(Constants.AppDeveloper);
        }

        private void BtnChangelog_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void BtnCheckUpdates_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void BtnDebug_OnClick(object sender, RoutedEventArgs e)
        {
            App.Manager.MainPageManager.AddMainPage(new DebugPages.DebugPage());
            btnDebug.IsEnabled = false;
        }

        private void BtnAdmin_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(
                new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace(".dll", ".exe")) //Net Core tell's us the location of the dll, not the exe so we point it back to the exe
                {
                    UseShellExecute = true,
                    Verb = "runas"
                });
                Application.Current.Shutdown();
            }
            catch (Win32Exception ex)
            {
                _ = CustomMessageBox.Show(ex.Message);
                if (ex.NativeErrorCode == 1223)
                {
                    NotificationCenter.Logger.Warning("User wanted to run Client as Admin but then changed their mind");
                }
                else
                {
                    NotificationCenter.Logger.Error("Something happened while trying to reload client as admin");
                }
            }
        }

        private void MaxHeightChanged(object sender, ResourceChangeEventArgs e)
        {
            if (e.NewValue is double doc) 
            {
                MaxHeight = 380 > doc ? 380 : doc;
            }
        }
    }
}
