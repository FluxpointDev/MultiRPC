using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DiscordRPC;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for MultiRPCPage.xaml
    /// </summary>
    public partial class MultiRPCPage : Page
    {
        public static MultiRPCPage _MultiRPCPage;
        private readonly RPCPreview _preview;
        private bool _haveDoneAutoStart;

        public MultiRPCPage()
        {
            InitializeComponent();
            UpdateText();
            _MultiRPCPage = this;

            _preview = new RPCPreview(RPCPreview.ViewType.Blank, backgroundName: "Purple", foreground: Brushes.White);
            frmRPCPreview.Content = _preview;

            tbText1.Text = App.Config.MultiRPC.Text1;
            tbText2.Text = App.Config.MultiRPC.Text2;
            tbLargeText.Text = App.Config.MultiRPC.LargeText;
            tbSmallText.Text = App.Config.MultiRPC.SmallText;
            cbElapasedTime.IsChecked = App.Config.MultiRPC.ShowTime;

            cbSmallKey.ItemsSource = Data.MultiRPCImages.Keys;
            cbLargeKey.ItemsSource = Data.MultiRPCImages.Keys;
            cbSmallKey.SelectedIndex = App.Config.MultiRPC.SmallKey;
            cbLargeKey.SelectedIndex = App.Config.MultiRPC.LargeKey;
        }

        private async void MultiRPCPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!RPC.IsRPCRunning)
            {
                RPC.IDToUse = RPC.MultiRPCID;
                RPC.UpdateType(RPC.RPCType.MultiRPC);
                RPC.SetPresence(tbText1.Text, tbText2.Text, cbLargeKey.Text.ToLower(), tbLargeText.Text,
                    cbSmallKey.Text.ToLower(), tbSmallText.Text, cbElapasedTime.IsChecked.Value);
            }

            if (!_haveDoneAutoStart && App.Config.AutoStart == "MultiRPC" && !App.StartedWithJumpListLogic)
            {
                if (await CanRunRPC())
                {
                    MainPage._MainPage.btnStart.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            }
            else
            {
                CanRunRPC();
            }

            IsEnabled = true;
            _haveDoneAutoStart = true;
        }

        private async Task UpdatePreviewImage(bool updateSmall, string key = "")
        {
            var value = key;
            if (string.IsNullOrWhiteSpace(key))
            {
                value = updateSmall ? Data.GetImageValue(cbSmallKey.Text) : Data.GetImageValue(cbLargeKey.Text);
            }
            else
            {
                value = Data.GetImageValue(key);
            }

            var image = !string.IsNullOrWhiteSpace(value) ? await new Uri(value).DownloadImage() : null;
            if (!string.IsNullOrWhiteSpace(value) && key != App.Text.NoImage)
            {
                _preview.UpdateImage(updateSmall, new ImageBrush(image));
            }
            else
            {
                _preview.UpdateImage(updateSmall, null);
            }
        }

        public Task UpdateText()
        {
            tblText1.Text = App.Text.Text1 + ":";
            tblText2.Text = App.Text.Text2 + ":";
            tblLargeKey.Text = App.Text.LargeKey + ":";
            tblLargeText.Text = App.Text.LargeText + ":";
            tblSmallKey.Text = App.Text.SmallKey + ":";
            tblSmallText.Text = App.Text.SmallText + ":";
            tblElapasedTime.Text = App.Text.ShowElapsedTime + ":";
            tblWhatWillLookLike.Text = App.Text.WhatItWillLookLike;

            return Task.CompletedTask;
        }

        public async Task<bool> CanRunRPC()
        {
            return await MultiRPCAndCustomLogic.CanRunRPC(tbText1, tbText2, tbSmallText, tbLargeText);
        }

        private async void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = await MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            if (RPC.Presence.Assets != null)
            {
                RPC.Presence.Assets.LargeImageText = tbLargeText.Text;
            }

            CanRunRPC();
            _preview.recLargeImage.ToolTip = !string.IsNullOrWhiteSpace(text) ? new ToolTip(text) : null;

            App.Config.MultiRPC.LargeText = tbLargeText.Text;
            App.Config.Save();
        }

        private async void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = await MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            if (RPC.Presence.Assets != null)
            {
                RPC.Presence.Assets.SmallImageText = tbSmallText.Text;
            }

            CanRunRPC();
            _preview.ellSmallImage.ToolTip = !string.IsNullOrWhiteSpace(text) ? new ToolTip(text) : null;

            App.Config.MultiRPC.SmallText = tbSmallText.Text;
            App.Config.Save();
        }

        public async void TbText1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MainPage._MainPage.btnUpdate.IsEnabled = false;
            MainPage._MainPage.btnStart.IsEnabled = false;

            var text = await MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            RPC.Presence.Details = text;
            _preview.tblText1.Text = text;
            _preview.UpdateTextVisibility();
            CanRunRPC();

            App.Config.MultiRPC.Text1 = tbText1.Text;
            App.Config.Save();
        }

        private async void TbText2_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = await MultiRPCAndCustomLogic.CheckImageText((TextBox) sender);

            RPC.Presence.State = text;
            _preview.tblText2.Text = text;
            _preview.UpdateTextVisibility();
            CanRunRPC();

            App.Config.MultiRPC.Text2 = tbText2.Text;
            App.Config.Save();
        }

        private async void CbElapasedTime_OnChecked(object sender, RoutedEventArgs e)
        {
            await MultiRPCAndCustomLogic.UpdateTimestamps((CheckBox) sender);
            App.Config.MultiRPC.ShowTime = ((CheckBox) sender).IsChecked.Value;
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

                for (var i = 0; i < cbLargeKey.Items.Count; i++)
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

        private Task UpdateImageEvent(SelectionChangedEventArgs e, bool updateSmall)
        {
            var image = e.AddedItems[0].ToString();
            if (IsEnabled)
            {
                UpdatePreviewImage(updateSmall, image);
            }

            if (RPC.Presence.Assets == null)
            {
                RPC.Presence.Assets = new Assets();
            }

            if (updateSmall)
            {
                RPC.Presence.Assets.SmallImageKey = image.ToLower();
            }
            else
            {
                RPC.Presence.Assets.LargeImageKey = image.ToLower();
            }

            return Task.CompletedTask;
        }
    }
}