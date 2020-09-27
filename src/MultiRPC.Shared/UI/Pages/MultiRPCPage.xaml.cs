using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MultiRPC.Core;
using static MultiRPC.Core.LanguagePicker;
using MultiRPC.Shared.UI.Views;
using MultiRPC.Core.Rpc;

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

        public MultiRPCPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
            Loaded += (sender, _) => Accessed?.Invoke(sender, null);
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
        }

        public string IconLocation => "Icon/Page/Discord";

        public string LocalizableName => "MultiRPC";

        public void txtText1_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.State = txtText1.Text;

        public void txtText2_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Details = txtText2.Text;

        public void txtSmallText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.LargeImage.Text = txtSmallText.Text;

        public void txtLargeText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.LargeImage.Text = txtLargeText.Text;

        public void cboLargeKey_SelectionChanged(object sender, RoutedEventArgs args) =>
            RichPresence.Assets.LargeImage.Key = cboLargeKey.SelectedItem.ToString();

        public void cboSmallKey_SelectionChanged(object sender, RoutedEventArgs args) =>
            RichPresence.Assets.SmallImage.Key = cboSmallKey.SelectedItem.ToString();

        public void cbElapasedTime_CheckedChanged(object sender, RoutedEventArgs args) =>
            RichPresence.Timestamp = new Timestamp();
    }
}
