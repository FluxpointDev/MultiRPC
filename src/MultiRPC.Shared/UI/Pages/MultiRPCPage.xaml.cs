using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MultiRPC.Core;
using MultiRPC.Core.Rpc;
using static MultiRPC.Core.LanguagePicker;
using MultiRPC.Shared.UI.Views;
using System.ComponentModel;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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

        //TODO: Make change based on vail
        public bool AllowStartingRPC => true;

        public void txtText1_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Details = txtText1.Text;

        public void txtText2_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.State = txtText2.Text;

        public void txtSmallText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.SmallImage.Text = txtSmallText.Text;

        public void txtLargeText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.LargeImage.Text = txtLargeText.Text;

        public void cboLargeKey_SelectionChanged(object sender, RoutedEventArgs args) =>
            RichPresence.Assets.LargeImage.Key = cboLargeKey.SelectedItem.ToString().ToLower();

        public void cboSmallKey_SelectionChanged(object sender, RoutedEventArgs args) =>
            RichPresence.Assets.SmallImage.Key = cboSmallKey.SelectedItem.ToString().ToLower();

        public void cbElapasedTime_CheckedChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Timestamp.Start = null;
            RichPresence.Timestamp.SetStartOnRPCConnection = 
                cbElapasedTime.IsChecked.GetValueOrDefault();
        }
    }
}
