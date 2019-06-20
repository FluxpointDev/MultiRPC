using System.Collections.Generic;
using System.Diagnostics;
using System.Extra;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public static List<UIText> UIText;

        public SettingsPage()
        {
            InitializeComponent();
            UpdateText();
            cbClient.SelectedIndex = App.Config.ClientToUse;

            ComboBoxItem item = null;
            for (var i = 0; i < cbAutoStart.Items.Count; i++)
            {
                var startItem = (ComboBoxItem) cbAutoStart.Items[i];
                if (startItem.Content.ToString() == App.Config.AutoStart)
                {
                    item = startItem;
                    break;
                }
            }

            if (item != null)
                cbAutoStart.SelectedItem = item;

            cbAfkTime.IsChecked = App.Config.AFKTime;
            cbDiscordCheck.IsChecked = !App.Config.DiscordCheck;
            cbTokenCheck.IsChecked = !App.Config.CheckToken;
            cbHelpIcons.IsChecked = App.Config.Disabled.HelpIcons;
            cbAutoUpdating.IsChecked = !App.Config.AutoUpdate;
            cbHideTaskbarIcon.IsChecked = !App.Config.HideTaskbarIconWhenMin;
            rAppDev.Text = App.AppDev;

            var activeLangInt = 0;
            var cbilangs = new List<ComboBoxItem>(UIText.Count);
            for (var i = 0; i < UIText.Count; i++)
            {
                var cbilang = new ComboBoxItem
                {
                    Content = UIText[i].LanguageName,
                    Tag = UIText[i].LanguageTag
                };
                cbilangs.Add(cbilang);

                if (UIText[i].LanguageTag == App.Config.ActiveLanguage) activeLangInt = i;
            }

            cbLanguage.ItemsSource = cbilangs;
            cbLanguage.SelectedIndex = activeLangInt;
            imgDownloadRPC.Tag = App.MultiRPCWebsiteRoot;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            btnChangelog.IsEnabled = File.Exists(FileLocations.ChangelogFileLocalLocation);
            UpdateButtonCheckLogic();
        }

        private async Task UpdateButtonCheckLogic()
        {
            btnCheckUpdates.IsEnabled = false;
            if (Updater.BeenUpdated || Updater.IsUpdating)
                return;

            await Task.Delay(250);
            while (Updater.IsChecking)
                await Task.Delay(250);

            btnCheckUpdates.IsEnabled = true;
        }

        private Task UpdateText()
        {
            tblWebsiteMultiRPC.Text = App.Text.Website;
            tblDownload.Text = App.Text.Download;
            tblDiscordServer.Text = App.Text.DiscordServer;
            tblJoinForFunBotsAndSupport.Text = App.Text.JoinForFunBotsAndSupport;
            tblWebsiteDiscord.Text = App.Text.Website;
            tblStatus.Text = App.Text.Status;
            tblSettings.Text = App.Text.Settings;
            tblClient.Text = App.Text.Client + ":";
            tblAutoStart.Text = App.Text.AutoStart + ":";
            tblAfkTime.Text = App.Text.AfkTime + ":";
            cbiAuto.Content = App.Text.Auto;
            cbiNo.Content = App.Text.No;
            cbiCustom.Content = App.Text.Custom;
            tblDisable.Text = App.Text.Disable;
            tblDiscordCheck.Text = App.Text.DiscordCheck + ":";
            tblTokenCheck.Text = App.Text.TokenCheck + ":";
            tblHelpIcons.Text = App.Text.HelpIcons + ":";
            tblAutoUpdating.Text = App.Text.AutomaticUpdates + ":";
            tblPaypal.Text = App.Text.PaypalMin1;
            tblPatreon.Text = App.Text.PatreonMonthly;
            btnPaypal.Content = App.Text.ClickHere;
            btnPatreon.Content = App.Text.ClickHere;
            tblDonations.Text = App.Text.Donations;
            tblDonateMessage.Text = App.Text.DonateMessage;
            btnChangelog.Content = App.Text.Changelog;
            btnCheckUpdates.Content = App.Text.CheckForUpdates;
            btnDebug.Content = App.Text.Debug;
            rMadeBy.Text = App.Text.MadeBy + ": ";
            tblLanguage.Text = App.Text.Language;
            rAppDev.ToolTip = new ToolTip(App.Text.ClickToCopy);
            tblHideTaskbarIcon.Text = App.Text.HideTaskbarIcon;
            tblShowPageTooltips.Text = App.Text.ShowPageTooltips;

            return Task.CompletedTask;
        }

        private void Image_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image) sender;
            Animations.DoubleAnimation(image, 0.8, image.Opacity);
        }

        private void Image_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var image = (Image) sender;
            Animations.DoubleAnimation(image, 0.6, image.Opacity);
        }

        private async void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((Image) sender).Tag.ToString());
            var image = (Image) sender;
            await Animations.DoubleAnimation(image, 1, image.Opacity);
            Animations.DoubleAnimation(image, 0.8, image.Opacity);
        }

        private async void Server_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            await CustomMessageBox.Show(App.Text.JoinServerMessage,
                App.Text.DiscordServer,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Process.Start(new [] { "https://discord.gg", App.ServerInviteCode }.Combine());
        }

        private void CbClient_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.ClientToUse = ((ComboBox) sender).SelectedIndex;
                App.Config.Save();
            }
        }

        private void CbAutoStart_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.AutoStart = ((ComboBoxItem) e.AddedItems[0]).Content.ToString();
                App.Config.Save();
            }
        }

        private void CbAfkTime_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.AFKTime = ((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private void CbDiscordCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.DiscordCheck = !((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private void CbTokenCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.CheckToken = !((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private void CbHelpIcons_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.Disabled.HelpIcons = ((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private async void ButPaypal_OnClick(object sender, RoutedEventArgs e)
        {
            await CustomMessageBox.Show(App.Text.PaypalDonateMessage);
        }

        private void ButPatreon_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/builderb");
        }

        private void ButChangelog_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenWindow(new ChangelogPage(), true, 0, false);
        }

        private void ButCheckUpdates_OnClick(object sender, RoutedEventArgs e)
        {
            Updater.Check(true);
            UpdateButtonCheckLogic();
        }

        private void ButDebug_OnClick(object sender, RoutedEventArgs e)
        {
            MainPage._MainPage.btnDebug.Visibility = Visibility.Visible;
            btnDebug.IsEnabled = false;
        }

        private void RAppDev_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(App.AppDev);
        }

        private void CbAutoUpdating_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.AutoUpdate = !((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private void CbLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                var oldActiveWord = App.Text.Active;
                var oldEditingWord = App.Text.Editing;
                var languageSelected = (ComboBoxItem) e.AddedItems[0];
                var internetConnectivityTextIsInternetLost =
                    MainPage._MainPage.tblInternetConnectivity.Text == App.Text.InternetLost;

                for (var i = 0; i < UIText.Count; i++)
                    if (UIText[i].LanguageTag == languageSelected.Tag.ToString())
                    {
                        App.Text = UIText[i];
                        break;
                    }

                App.Config.ActiveLanguage = languageSelected.Tag.ToString();

                UpdateText();
                App.Config.AutoStart = cbAutoStart.Text;
                MainPage._MainPage.UpdateText();
                MultiRPCPage._MultiRPCPage?.UpdateText();
                MasterCustomPage._MasterCustomPage?.UpdateText();
                ProgramsPage._ProgramsPage?.UpdateText();
                CreditsPage._CreditsPage?.UpdateText();
                MasterThemeEditorPage._MasterThemeEditorPage?.UpdateText();
                ThemeEditorPage._ThemeEditorPage?.UpdateText();
                InstalledThemes._InstalledThemes?.UpdateText(oldEditingWord, oldActiveWord);
                DebugPage._DebugPage?.UpdateText();

                if (!string.IsNullOrWhiteSpace(MainPage._MainPage.tblInternetConnectivity.Text))
                    MainPage._MainPage.tblInternetConnectivity.Text = internetConnectivityTextIsInternetLost
                        ? App.Text.InternetLost
                        : App.Text.InternetBack;

                Data.MultiRPCImages = Data.MakeImagesDictionary();

                if (MultiRPCPage._MultiRPCPage != null)
                {
                    MultiRPCPage._MultiRPCPage.cbSmallKey.ItemsSource = Data.MultiRPCImages.Keys;
                    MultiRPCPage._MultiRPCPage.cbLargeKey.ItemsSource = Data.MultiRPCImages.Keys;
                    MultiRPCPage._MultiRPCPage.cbSmallKey.Items.Refresh();
                    MultiRPCPage._MultiRPCPage.cbLargeKey.Items.Refresh();
                    MultiRPCPage._MultiRPCPage.cbSmallKey.SelectedIndex = App.Config.MultiRPC.SmallKey;
                    MultiRPCPage._MultiRPCPage.cbLargeKey.SelectedIndex = App.Config.MultiRPC.LargeKey;
                }

                App.Config.Save();
            }
        }

        private void CbHideTaskbarIcon_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.HideTaskbarIconWhenMin = !((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private void CbShowPageTooltips_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.ShowPageTooltips = !((CheckBox) sender).IsChecked.Value;
                App.Config.Save();
                MainPage._MainPage.UpdateTooltips();
            }
        }
    }
}