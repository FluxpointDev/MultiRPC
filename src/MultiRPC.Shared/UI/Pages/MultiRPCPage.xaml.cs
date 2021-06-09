using System;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using static MultiRPC.Core.LanguagePicker;
using MultiRPC.Shared.UI.Views;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MultiRPC.Core.Page;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MultiRPCPage : LocalizablePage, ISidePage, IRpcPage
    {
        public RichPresence RichPresence { get; } = new RichPresence("MultiRPC", Constants.MultiRPCID);

        public event EventHandler Accessed;
        public event PropertyChangedEventHandler PropertyChanged;
        private Brush DefaultBorder;

        public MultiRPCPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
            Loaded += (sender, _) =>
            {
                rpcView.RichPresence = RichPresence;
                Accessed?.Invoke(sender, null);
            };
        }

        private void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DefaultBorder ??= txtText1.BorderBrush; //TODO: Find a better way
            var redBorder = Application.Current.Resources["Red"];

            switch (e.PropertyName)
            {
                case nameof(RichPresence.Presence.Details):
                    txtText1.SetValue(BorderBrushProperty, txtText1.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText1, txtText1.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.State):
                    txtText2.SetValue(BorderBrushProperty, txtText2.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText2, txtText2.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.Assets.LargeImageText): //Check Small + Large
                    txtLargeText.SetValue(BorderBrushProperty, txtLargeText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtLargeText, txtLargeText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);

                    txtSmallText.SetValue(BorderBrushProperty, txtSmallText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtSmallText, txtSmallText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
            }

            PropertyChanged?.Invoke(this, e);
        }

        public override void UpdateText()
        {
            tblText1.Text = $"{GetLineFromLanguageFile("Text1")}:";
            tblText2.Text = $"{GetLineFromLanguageFile("Text2")}:";
            tblLargeKey.Text = $"{GetLineFromLanguageFile("LargeKey")}:";
            tblLargeText.Text = $"{GetLineFromLanguageFile("LargeText")}:";
            tblSmallKey.Text = $"{GetLineFromLanguageFile("SmallKey")}:";
            tblSmallText.Text = $"{GetLineFromLanguageFile("SmallText")}:";
            tblElapasedTime.Text = $"{GetLineFromLanguageFile("ShowElapsedTime")}:";
            tblWhatWillLookLike.Text = GetLineFromLanguageFile("WhatItWillLookLike");

            //Get the MultiRPCImages, need to temp store where we was as we will lose
            //the index on changing it
            var oldLargeIndex = cboLargeKey.SelectedIndex;
            var oldSmallIndex = cboSmallKey.SelectedIndex;
            cboLargeKey.ItemsSource = Data.MultiRPCImages.Keys;
            cboSmallKey.ItemsSource = Data.MultiRPCImages.Keys;
            cboLargeKey.SelectedIndex = oldLargeIndex;
            cboSmallKey.SelectedIndex = oldSmallIndex;
        }

        public string IconLocation => "Icon/Page/Discord";

        public string LocalizableName => "MultiRPC";

        public bool VaildRichPresence => txtText1.Text.Length != 1 && txtText2.Text.Length != 1 && txtLargeText.Text.Length != 1 && txtSmallText.Text.Length != 1;

        public void txtText1_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Details = txtText1.Text;

        public void txtText2_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.State = txtText2.Text;

        public void txtSmallText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Assets.SmallImageText = txtSmallText.Text;

        public void txtLargeText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Assets.LargeImageText = txtLargeText.Text;

        public void cboLargeKey_SelectionChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Presence.Assets.LargeImageKey = cboLargeKey.SelectedItem.ToString().ToLower();
            //RichPresence.Assets.LargeImage.Uri = Data.GetImageValue(cboLargeKey.SelectedItem.ToString());
        }

        public void cboSmallKey_SelectionChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Presence.Assets.SmallImageKey = cboSmallKey.SelectedItem.ToString().ToLower();
            //RichPresence.Assets.SmallImage.Uri = Data.GetImageValue(cboSmallKey.SelectedItem.ToString());
        }

        public void cbElapasedTime_CheckedChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Presence.Timestamps.Start = 
                cbElapasedTime.IsChecked.GetValueOrDefault() 
                    ? DateTime.MinValue 
                    : null;
        }
    }
}
