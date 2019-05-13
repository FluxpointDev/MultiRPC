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
        private RPCPreview preview;
        public static MultiRPCPage _MultiRPCPage;
        private bool haveDoneAutoStart;

        public MultiRPCPage()
        {
            InitializeComponent();
            Loaded += MultiRPCPage_Loaded;
            _MultiRPCPage = this;

            preview = new RPCPreview(RPCPreview.ViewType.Default, backgroundName: "Purple", foreground: Brushes.White);
            preview.tblText1.Text = "";
            preview.tblText2.Text = "";
            cbElapasedTime.Unchecked += CbElapasedTime_OnChecked;
            cbElapasedTime.Checked += CbElapasedTime_OnChecked;

            frameRPCPreview.Content = preview;

            tbText1.Text = App.Config.MultiRPC.Text1;
            tbText2.Text = App.Config.MultiRPC.Text2;
            tbLargeText.Text = App.Config.MultiRPC.LargeText;
            tbSmallText.Text = App.Config.MultiRPC.SmallText;
            cbElapasedTime.IsChecked = App.Config.MultiRPC.ShowTime;
        }

        private async void MultiRPCPage_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateText();
            Data.MultiRPCImages = await Data.MakeImagesDictionary();

            cbSmallKey.ItemsSource = Data.MultiRPCImages.Keys;
            cbLargeKey.ItemsSource = Data.MultiRPCImages.Keys;
            cbSmallKey.Items.Refresh();
            cbLargeKey.Items.Refresh();
            cbSmallKey.SelectedIndex = App.Config.MultiRPC.SmallKey;
            cbLargeKey.SelectedIndex = App.Config.MultiRPC.LargeKey;

            RPC.IDToUse = RPC.MuiltiRPCID;
            RPC.UpdateType(RPC.RPCType.MultiRPC);
            RPC.SetPresence(tbText1.Text, tbText2.Text, cbLargeKey.Text.ToLower(), tbLargeText.Text, cbSmallKey.Text.ToLower(), tbSmallText.Text, cbElapasedTime.IsChecked.Value);

            if (!haveDoneAutoStart && App.Config.AutoStart == "MultiRPC")
            {
                if (await CanRunRPC())
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                haveDoneAutoStart = true;
            }
            else
            {
                CanRunRPC();
            }
            IsEnabled = true;
        }

        private async Task UpdatePreviewImage(bool updateSmall, string key = "")
        {
            string value = key;
            if (string.IsNullOrWhiteSpace(key))
                value = updateSmall ? Data.GetImageValue(cbSmallKey.Text) : Data.GetImageValue(cbLargeKey.Text);
            else
                value = Data.GetImageValue(key);

            var image = !string.IsNullOrWhiteSpace(value) ? await BitmapDownloader.DownloadImage(new Uri(value)) : null;
            if (!string.IsNullOrWhiteSpace(value) && key != App.Text.NoImage)
                preview.UpdateImage(updateSmall, new ImageBrush(image));
            else
                preview.UpdateImage(updateSmall, null);
        }

        private async Task UpdateText()
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

        public async Task<bool> CanRunRPC()
        {
            return await MultiRPCAndCustomLogic.CanRunRPC(tbText1, tbText2, tbSmallText, tbLargeText);
        }

        private async void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = await MultiRPCAndCustomLogic.CheckImageText(((TextBox)sender));

            if (RPC.Presence.Assets != null)
                RPC.Presence.Assets.LargeImageText = tbLargeText.Text;
            CanRunRPC();
            preview.recLargeImage.ToolTip = !string.IsNullOrWhiteSpace(text) ? new ToolTip(text) : null;

            App.Config.MultiRPC.LargeText = tbLargeText.Text;
            App.Config.Save();
        }

        private async void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = await MultiRPCAndCustomLogic.CheckImageText(((TextBox)sender));

            if(RPC.Presence.Assets != null)
                RPC.Presence.Assets.SmallImageText = tbSmallText.Text;
            CanRunRPC();
            preview.ellSmallImage.ToolTip = !string.IsNullOrWhiteSpace(text) ? new ToolTip(text) : null;

            App.Config.MultiRPC.SmallText = tbSmallText.Text;
            App.Config.Save();
        }

        private async void TbText1_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            MainPage._MainPage.btnUpdate.IsEnabled = false;
            MainPage._MainPage.btnStart.IsEnabled = false;

            string text = await MultiRPCAndCustomLogic.CheckImageText((TextBox)sender);

            RPC.Presence.Details = text;
            preview.tblText1.Text = text;
            preview.UpdateTextVisibility();
            CanRunRPC();

            App.Config.MultiRPC.Text1 = tbText1.Text;
            App.Config.Save();
        }

        private async void TbText2_OnSizeChanged(object sender, TextChangedEventArgs e)
        {
            string text = await MultiRPCAndCustomLogic.CheckImageText((TextBox)sender);

            RPC.Presence.State = text;
            preview.tblText2.Text = text;
            preview.UpdateTextVisibility();
            CanRunRPC();

            App.Config.MultiRPC.Text2 = tbText2.Text;
            App.Config.Save();
        }

        public async void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            await MultiRPCAndCustomLogic.UpdateTimestamps((CheckBox)sender);
            App.Config.MultiRPC.ShowTime = ((CheckBox)sender).IsChecked.Value;
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
            if (IsEnabled)
                UpdatePreviewImage(UpdateSmall, image);

            if (RPC.Presence.Assets == null)
                RPC.Presence.Assets = new Assets();
            if (UpdateSmall)
                RPC.Presence.Assets.SmallImageKey = image.ToLower();
            else
                RPC.Presence.Assets.LargeImageKey = image.ToLower();
        }
    }
}
