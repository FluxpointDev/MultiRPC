using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using MultiRPC.JsonClasses;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public static List<UIText> UIText;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
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
            rAppDev.Text = App.AppDev;
            cbTheme.SelectedIndex = (int)App.Config.ActiveTheme;

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
            butChangelog.IsEnabled = File.Exists(FileLocations.ChangelogFileLocalLocation);
            UpdateButtonCheckLogic();
        }

        public async Task UpdateButtonCheckLogic()
        {
            butCheckUpdates.IsEnabled = false;
            if (Updater.BeenUpdated || Updater.IsUpdating)
                return;

            await Task.Delay(250);
            while (Updater.IsChecking)
                await Task.Delay(250);

            butCheckUpdates.IsEnabled = true;
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
            butPaypal.Content = App.Text.ClickHere;
            butPatreon.Content = App.Text.ClickHere;
            tblDonations.Text = App.Text.Donations;
            tblDonateMessage.Text = App.Text.DonateMessage;
            butChangelog.Content = App.Text.Changelog;
            butCheckUpdates.Content = App.Text.CheckForUpdates;
            butDebug.Content = App.Text.Debug;
            tblTheme.Text = App.Text.Theme;
            cbiDark.Content = App.Text.Dark;
            cbiLight.Content = App.Text.Light;
            rMadeBy.Text = App.Text.MadeBy + ": ";
            tblLanguage.Text = App.Text.Language;
            rAppDev.ToolTip = new ToolTip(App.Text.ClickToCopy);
            tblHideTaskbarIcon.Text = App.Text.HideTaskbarIcon;
        }

        private void Image_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((Image) sender).Opacity = 1;
        }

        private void Image_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 0.6;
        }

        private void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((Image) sender).Tag.ToString());
        }

        private void Server_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(App.Text.JoinServerMessage,
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

        private void ButPaypal_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(App.Text.PaypalDonateMessage);
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
            MainPage.mainPage.butDebug.Visibility = Visibility.Visible;
            butDebug.IsEnabled = false;
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

        private async void CbTheme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                string reName = null;
                var active = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
                if (active == App.Text.Light)
                {
                    App.Config.ActiveTheme = Theme.ActiveTheme.Light;
                    reName = "Light";
                }
                else if (active == App.Text.Dark)
                {
                    App.Config.ActiveTheme = Theme.ActiveTheme.Dark;
                    reName = "Dark";
                }
                else // if (active == App.Text.Dark)
                {
                    App.Config.ActiveTheme = Theme.ActiveTheme.Custom;
                }
                App.Config.Save();

                while (MainPage.mainPage.frameRPCPreview.Content == null)
                    await Task.Delay(250);

                App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/{reName}Theme.xaml")));
                App.Current.Resources.MergedDictionaries.Remove(App.Current.Resources.MergedDictionaries[1]);
                App.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));
                App.Current.Resources.MergedDictionaries.Remove(App.Current.Resources.MergedDictionaries[0]);
                var frameRPCPreviewBG = MainPage.mainPage.frameRPCPreview.Content != null ? ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).gridBackground.Background : ((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
                if (((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]).Color && ((SolidColorBrush)frameRPCPreviewBG).Color != ((SolidColorBrush)Application.Current.Resources["Red"]).Color)
                {
                    ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateBackground(
                        (SolidColorBrush)App.Current.Resources["AccentColour2SCBrush"]);
                    ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateForground(
                        (SolidColorBrush)App.Current.Resources["TextColourSCBrush"]);
                }
                
                MainPage.mainPage.RerenderButtons();
                ((MainWindow)App.Current.MainWindow).TaskbarIcon.TrayToolTip = new ToolTip(App.Current.MainWindow.WindowState == WindowState.Minimized ? App.Text.ShowMultiRPC : App.Text.HideMultiRPC);
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
