using System;
using DiscordRPC;
using System.Windows;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for MuiltiRPCPage.xaml
    /// </summary>
    public partial class MultiRPCPage : Page
    {
        private RPCPreview Preview;
        private static MultiRPCPage _page;
        public static MultiRPCPage multiRpcPage => _page;

        public MultiRPCPage()
        {
            InitializeComponent();
            UpdateText();
            Loaded += MuiltiRPCPage_Loaded;
            _page = this;

            Preview = new RPCPreview(RPCPreview.ViewType.Default, background: (SolidColorBrush)App.Current.Resources["Purple"], forground: Brushes.White);
            Preview.tblText1.Text = "";
            Preview.tblText2.Text = "";
            cbElapasedTime.Unchecked += CbElapasedTime_OnChecked;
            cbElapasedTime.Checked += CbElapasedTime_OnChecked;

            frameRPCPreview.Content = Preview;

            tbText1.Text = App.Config.MultiRPC.Text1;
            tbText2.Text = App.Config.MultiRPC.Text2;
            tbLargeText.Text = App.Config.MultiRPC.LargeText;
            tbSmallText.Text = App.Config.MultiRPC.SmallText;
            cbLargeKey.SelectedIndex = App.Config.MultiRPC.LargeKey;
            cbSmallKey.SelectedIndex = App.Config.MultiRPC.SmallKey;
            cbElapasedTime.IsChecked = App.Config.MultiRPC.ShowTime;
        }

        private async void MuiltiRPCPage_Loaded(object sender, RoutedEventArgs e)
        {
            Data.MultiRPCImages = await Data.MakeImagesDictionary();
            cbSmallKey.ItemsSource = Data.MultiRPCImages.Keys;
            cbLargeKey.ItemsSource = Data.MultiRPCImages.Keys;
            cbSmallKey.SelectedIndex = App.Config.MultiRPC.SmallKey;
            cbLargeKey.SelectedIndex = App.Config.MultiRPC.LargeKey;
            cbSmallKey.Items.Refresh();
            cbLargeKey.Items.Refresh();

            RPC.IDToUse = RPC.MuiltiRPCID;
            RPC.UpdateType(RPC.RPCType.MultiRPC);
            RPC.SetPresence(tbText1.Text, tbText2.Text, cbLargeKey.Text.ToLower(), tbLargeText.Text, cbSmallKey.Text.ToLower(), tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            UpdatePreviewImage(false);
            UpdatePreviewImage(true);
            CanRunRPC();
            UpdateText();
        }

        public async Task UpdatePreviewImage(bool updateSmall, string key = "")
        {
            string value = key;
            if (string.IsNullOrWhiteSpace(key))
                value = updateSmall ? Data.GetImageValue(cbSmallKey.Text) : Data.GetImageValue(cbLargeKey.Text);
            else
                value = Data.GetImageValue(key);

            var image = !string.IsNullOrWhiteSpace(value) ? await BitmapDownloader.DownloadImage(new Uri(value)) : null;
            if (!string.IsNullOrWhiteSpace(value) && key != App.Text.NoImage)
                Preview.UpdateImage(updateSmall, new ImageBrush(image));
            else
                Preview.UpdateImage(updateSmall, null);
        }

        public async Task UpdateText()
        {
            tblText1.Text = App.Text.Text1 + ":";
            tblText2.Text = App.Text.Text2 + ":";
            tblLargeKey.Text = App.Text.LargeKey + ":";
            tblLargeText.Text = App.Text.LargeText + ":";
            tblSmallKey.Text = App.Text.SmallKey + ":";
            tblSmallText.Text = App.Text.SmallText + ":";
            tblElapasedTime.Text = App.Text.ShowElapsedTime + ":";
            tblWhatWillLookLike.Text = App.Text.WhatItWillLookLike;
        }

        private async void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = await CheckImageText(((TextBox)sender));

            if (RPC.Presence.Assets != null)
                RPC.Presence.Assets.LargeImageText = tbLargeText.Text;
            CanRunRPC();
            Preview.recLargeImage.ToolTip = !string.IsNullOrWhiteSpace(text) ? new Controls.ToolTip(text) : null;

            App.Config.MultiRPC.LargeText = tbLargeText.Text;
            App.Config.Save();
        }

        private async void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = await CheckImageText(((TextBox)sender));

            if(RPC.Presence.Assets != null)
                RPC.Presence.Assets.SmallImageText = tbSmallText.Text;
            CanRunRPC();
            Preview.ellSmallImage.ToolTip = !string.IsNullOrWhiteSpace(text) ? new Controls.ToolTip(text) : null;

            App.Config.MultiRPC.SmallText = tbSmallText.Text;
            App.Config.Save();
        }

        public async Task<string> CheckImageText(TextBox textBox)
        {
            string text = textBox.Text;
            if (!Checks.UnderAmountOfBytes(text, 128))
                textBox.Undo();
            return textBox.Text;
        }

        private void TbText1_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            MainPage.mainPage.btnUpdate.IsEnabled = false;
            MainPage.mainPage.btnStart.IsEnabled = false;

            string text = ((TextBox)sender).Text;

            if (!Checks.UnderAmountOfBytes(text, 128))
            {
                ((TextBox)sender).Undo();
                text = ((TextBox)sender).Text;
            }

            RPC.Presence.Details = text;
            Preview.tblText1.Text = text;
            Preview.UpdateTextVisibility();
            CanRunRPC();

            App.Config.MultiRPC.Text1 = tbText1.Text;
            App.Config.Save();
        }

        public async Task CanRunRPC()
        {
            bool isEnabled = true;
            if (tbText2.Text.Length == 1)
            {
                tbText2.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbText2.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText2.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbText2.ToolTip = null;
            }
            if (tbText1.Text.Length == 1)
            {
                tbText1.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbText1.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText1.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbText1.ToolTip = null;
            }

            if (tbSmallText.Text.Length == 1)
            {
                tbSmallText.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbSmallText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
            }
            else
            {
                tbSmallText.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbSmallText.ToolTip = null;
            }
            if (tbLargeText.Text.Length == 1)
            {
                tbLargeText.BorderBrush = (SolidColorBrush)App.Current.Resources["Red"];
                tbLargeText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
            }
            else
            {
                tbLargeText.BorderBrush = (SolidColorBrush)App.Current.Resources["AccentColour4SCBrush"];
                tbLargeText.ToolTip = null;
            }

            if (MainPage.mainPage.ContentFrame.Content is MultiRPCPage && RPC.Type == RPC.RPCType.MultiRPC)
            {
                if (MainPage.mainPage.btnStart.Content.ToString() == App.Text.Shutdown)
                {
                    MainPage.mainPage.btnUpdate.IsEnabled = isEnabled;
                    MainPage.mainPage.btnStart.IsEnabled = true;
                }
                else
                {
                    MainPage.mainPage.btnUpdate.IsEnabled = false;
                    MainPage.mainPage.btnStart.IsEnabled = isEnabled;
                }
            }
            else if (MainPage.mainPage.btnStart.Content.ToString() == App.Text.Shutdown)
            {
                MainPage.mainPage.btnUpdate.IsEnabled = false;
                MainPage.mainPage.btnStart.IsEnabled = true;
            }
        }

        private void TbText2_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            string text = ((TextBox)sender).Text;

            if (!Checks.UnderAmountOfBytes(text, 128))
            {
                ((TextBox) sender).Undo();
                text = ((TextBox) sender).Text;
            }

            RPC.Presence.State = text;
            Preview.tblText2.Text = text;
            Preview.UpdateTextVisibility();
            CanRunRPC();

            App.Config.MultiRPC.Text2 = tbText2.Text;
            App.Config.Save();
        }

        private async void CbSmallKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSmallKey.SelectedIndex != -1)
            {
                await UpdateImageEvent(e, true);

                App.Config.MultiRPC.SmallKey = cbSmallKey.SelectedIndex;
                App.Config.Save();
            }
        }

        private async void CbLargeKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLargeKey.SelectedIndex != -1)
            {
                await UpdateImageEvent(e, false);

                for (int i = 0; i < cbLargeKey.Items.Count; i++)
                {
                    if (cbLargeKey.Items[i] == e.AddedItems[0])
                    {
                        App.Config.MultiRPC.LargeKey = i;
                        break;
                    }
                }

                App.Config.Save();
            }
        }

        public async Task UpdateImageEvent(SelectionChangedEventArgs e, bool UpdateSmall)
        {
            string image = e.AddedItems[0].ToString();
            await UpdatePreviewImage(UpdateSmall, image);
            if (RPC.Presence.Assets == null)
                RPC.Presence.Assets = new Assets();
            if (UpdateSmall)
                RPC.Presence.Assets.SmallImageKey = image.ToLower();
            else
                RPC.Presence.Assets.LargeImageKey = image.ToLower();
        }

        public static async Task UpdateTimestamps(CheckBox checkBox)
        {
            if (checkBox.IsChecked.Value)
                RPC.Presence.Timestamps = new Timestamps();
            else
                RPC.Presence.Timestamps = null;
        }

        public async void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            await UpdateTimestamps((CheckBox)sender);
            App.Config.MultiRPC.ShowTime = ((CheckBox) sender).IsChecked.Value;
            App.Config.Save();
        }
    }
}
