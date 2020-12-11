using System;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using static MultiRPC.Core.LanguagePicker;
using MultiRPC.Shared.UI.Views;
using System.ComponentModel;
using MultiRPC.Core.Page;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
#endif

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
                ctcView.Content ??= new RPCView
                {
                    RichPresence = RichPresence, 
                    CurrentView = RPCView.ViewType.RichPresence 
                };
                Accessed?.Invoke(sender, null);
            };
            RichPresence.PropertyChanged += RichPresence_PropertyChanged;
            RichPresence.Assets.LargeImage.PropertyChanged += RichPresence_PropertyChanged;
            RichPresence.Assets.SmallImage.PropertyChanged += RichPresence_PropertyChanged;
        }

        private async void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DefaultBorder ??= txtText1.BorderBrush; //TODO: Find a better way
            var redBorder = Application.Current.Resources["Red"];

            switch (e.PropertyName)
            {
                case nameof(RichPresence.Details):
                    txtText1.SetValue(BorderBrushProperty, txtText1.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText1, txtText1.Text.Length == 1 ? await GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.State):
                    txtText2.SetValue(BorderBrushProperty, txtText2.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText2, txtText2.Text.Length == 1 ? await GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Assets.LargeImage.Text): //Check Small + Large
                    txtLargeText.SetValue(BorderBrushProperty, txtLargeText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtLargeText, txtLargeText.Text.Length == 1 ? await GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);

                    txtSmallText.SetValue(BorderBrushProperty, txtSmallText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtSmallText, txtSmallText.Text.Length == 1 ? await GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
            }

            PropertyChanged?.Invoke(this, e);
        }

        public override async void UpdateText()
        {
            tblText1.Text = $"{await GetLineFromLanguageFile("Text1")}:";
            tblText2.Text = $"{await GetLineFromLanguageFile("Text2")}:";
            tblLargeKey.Text = $"{await GetLineFromLanguageFile("LargeKey")}:";
            tblLargeText.Text = $"{await GetLineFromLanguageFile("LargeText")}:";
            tblSmallKey.Text = $"{await GetLineFromLanguageFile("SmallKey")}:";
            tblSmallText.Text = $"{await GetLineFromLanguageFile("SmallText")}:";
            tblElapasedTime.Text = $"{await GetLineFromLanguageFile("ShowElapsedTime")}:";
            tblWhatWillLookLike.Text = await GetLineFromLanguageFile("WhatItWillLookLike");

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
            RichPresence.Details = txtText1.Text;

        public void txtText2_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.State = txtText2.Text;

        public void txtSmallText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.SmallImage.Text = txtSmallText.Text;

        public void txtLargeText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.LargeImage.Text = txtLargeText.Text;

        public void cboLargeKey_SelectionChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Assets.LargeImage.Key = cboLargeKey.SelectedItem.ToString().ToLower();
            RichPresence.Assets.LargeImage.Uri = Data.GetImageValue(cboLargeKey.SelectedItem.ToString());
        }

        public void cboSmallKey_SelectionChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Assets.SmallImage.Key = cboSmallKey.SelectedItem.ToString().ToLower();
            RichPresence.Assets.SmallImage.Uri = Data.GetImageValue(cboSmallKey.SelectedItem.ToString());
        }

        public void cbElapasedTime_CheckedChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Timestamp.Start = null;
            RichPresence.Timestamp.SetStartOnRPCConnection = 
                cbElapasedTime.IsChecked.GetValueOrDefault();
        }
    }
}
