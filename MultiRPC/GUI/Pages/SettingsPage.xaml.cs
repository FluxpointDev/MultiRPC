using System;
using System.IO;
using System.Windows;
using MultiRPC.Functions;
using System.Diagnostics;
using System.Windows.Input;
using MultiRPC.JsonClasses;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public static List<UIText> UIText;
        private static SettingsPage _settingsPage;
        public static SettingsPage settingsPage => _settingsPage;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
            _settingsPage = this;
            UpdateText();
            cbClient.SelectedIndex = App.Config.ClientToUse;
            ComboBoxItem item = null;
            foreach (ComboBoxItem start in cbAutoStart.Items)
            {
                if (start.Content.ToString() == App.Config.AutoStart)
                {
                    item = start;
                    break;
                }
            }
            if(item != null)
                cbAutoStart.SelectedItem = item;
            cbAfkTime.IsChecked = App.Config.AFKTime;
            cbDiscordCheck.IsChecked = !App.Config.DiscordCheck;
            cbTokenCheck.IsChecked = !App.Config.CheckToken;
            cbProgramsTab.IsChecked = App.Config.Disabled.ProgramsTab;
            cbHelpIcons.IsChecked = App.Config.Disabled.HelpIcons;
            cbAutoUpdating.IsChecked = !App.Config.AutoUpdate;
            cbHideTaskbarIcon.IsChecked = !App.Config.HideTaskbarIconWhenMin;
            rAppDev.Text = App.AppDev;

            int ActiveLangInt = 0;
            string[] langs = new string[UIText.Count];
            for (int i = 0; i < UIText.Count; i++)
            {
                langs[i] = UIText[i].LanguageName;
                if (UIText[i].LanguageName == App.Config.ActiveLanguage)
                    ActiveLangInt = i;
            }
            cbLanguage.ItemsSource = langs;
            cbLanguage.SelectedIndex = ActiveLangInt;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            btnChangelog.IsEnabled = File.Exists(FileLocations.ChangelogFileLocalLocation);
            UpdateButtonCheckLogic();
        }

        public async Task UpdateButtonCheckLogic()
        {
            btnCheckUpdates.IsEnabled = false;
            if (Updater.BeenUpdated || Updater.IsUpdating)
                return;

            await Task.Delay(250);
            while (Updater.IsChecking)
                await Task.Delay(250);

            btnCheckUpdates.IsEnabled = true;
        }

        public async Task UpdateText()
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
            tblProgramsTab.Text = App.Text.ProgramsTab + ":";
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
        }

        public static void ImageAnimation(Image image, double to, Storyboard storyboard = null, Duration duration = new Duration())
        {
            if(storyboard == null)
                storyboard = new Storyboard();
            DoubleAnimation winOpacityAnimation = new DoubleAnimation();
            winOpacityAnimation.From = image.Opacity;
            winOpacityAnimation.To = to;
            winOpacityAnimation.Duration = !duration.HasTimeSpan ? new Duration(TimeSpan.FromSeconds(0.5)) : duration;
            winOpacityAnimation.EasingFunction = new QuinticEase();

            storyboard.Children.Add(winOpacityAnimation);
            Storyboard.SetTargetName(winOpacityAnimation, image.Name);
            Storyboard.SetTargetProperty(winOpacityAnimation, new PropertyPath(Image.OpacityProperty));
            storyboard.Begin(image);
        }

        private void Image_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ImageAnimation(((Image) sender), 1);
        }

        private void Image_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ImageAnimation(((Image)sender), 0.6);
        }

        private void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((Image) sender).Tag.ToString());
        }

        private async void Server_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            await CustomMessageBox.Show(App.Text.JoinServerMessage,
                App.Text.DiscordServer,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Process.Start("https://discord.gg/" + App.ServerInviteCode);
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
                App.Config.AutoStart = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
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

        private void CbProgramsTab_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.Disabled.ProgramsTab = ((CheckBox) sender).IsChecked.Value;
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
            MainWindow window = new MainWindow(new ChangelogPage(), false);
            window.ShowDialog();
        }

        private void ButCheckUpdates_OnClick(object sender, RoutedEventArgs e)
        {
            Updater.Check(true);
            UpdateButtonCheckLogic();
        }

        private void ButDebug_OnClick(object sender, RoutedEventArgs e)
        {
            MainPage.mainPage.btnDebug.Visibility = Visibility.Visible;
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
                App.Config.AutoUpdate = !((CheckBox)sender).IsChecked.Value;
                App.Config.Save();
            }
        }

        private async void CbLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var wew = e.AddedItems[0].ToString();
            foreach (var uiText in UIText)
            {
                if (uiText.LanguageName == wew)
                {
                    App.Text = uiText;
                    break;
                }
            }

            App.Config.ActiveLanguage = wew;
            await UpdateText();
            App.Config.AutoStart = cbAutoStart.Text;
            MainPage.mainPage.UpdateText();
            App.Config.Save();
        }

        private void CbHideTaskbarIcon_OnChecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                App.Config.HideTaskbarIconWhenMin = !((CheckBox)sender).IsChecked.Value;
                App.Config.Save();
            }
        }
    }
}
