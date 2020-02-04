using System;
using System.Windows;
using System.Windows.Controls;
using MultiRPC.Core;
using MultiRPC.Core.Enums;
using MultiRPC.Core.Extensions;
using MultiRPC.Core.Rpc;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.CorePages
{
    /// <summary>
    /// Interaction logic for MultiRPCPage.xaml
    /// </summary>
    public partial class MultiRPCPage : PageWithIcon
    {
        public override MultiRPCIcons IconName { get; } = MultiRPCIcons.Discord;
        public override string JsonContent => "MultiRPC";

        public MultiRPCPage()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                UpdateStartButtonText();
                Settings.Current.LanguageChanged += (sender, args) => UpdateStartButtonText();
                Rpc.ConnectionClosed += Rpc_ConnectionClosed;
                Rpc.ApplicationID = Constants.MultiRPCID;
            };
            UpdateText();
            cbSmallKey.ItemsSource = Data.MultiRPCImages;
            cbLargeKey.ItemsSource = cbSmallKey.ItemsSource;
            cbSmallKey.SelectedIndex = Settings.Current.MultiRPC.SmallKey;
            cbLargeKey.SelectedIndex = Settings.Current.MultiRPC.LargeKey;

            tbText1.Text = Settings.Current.MultiRPC.Text1;
            tbText2.Text = Settings.Current.MultiRPC.Text2;
            tbLargeText.Text = Settings.Current.MultiRPC.LargeText;
            tbSmallText.Text = Settings.Current.MultiRPC.SmallText;
            cbElapasedTime.IsChecked = Settings.Current.MultiRPC.ShowTime;

            Settings.Current.LanguageChanged += LanguageChanged;
            Unloaded += (sender, args) =>
            {
                Settings.Current.LanguageChanged -= (_, __) => UpdateStartButtonText();
                Rpc.ConnectionClosed -= Rpc_ConnectionClosed;
            };
            Settings.Current.MultiRPC.PropertyChanged += MultiRPC_PropertyChanged;
            MultiRPC_PropertyChanged(Settings.Current.MultiRPC, null);

            tbText1.TextChanged += (_, __) => CheckIfRpcCanRun();
            tbText2.TextChanged += (_, __) => CheckIfRpcCanRun();
            tbLargeText.TextChanged += (_, __) => CheckIfRpcCanRun();
            tbSmallText.TextChanged += (_, __) => CheckIfRpcCanRun();
        }

        private void Rpc_ConnectionClosed(object sender, EventArgs e)
        {
            Rpc.ApplicationID = Constants.MultiRPCID;
        }

        private void LanguageChanged(object sender, EventArgs args)
        {
            UpdateText();
            var smallInt = cbSmallKey.SelectedIndex;
            var largeInt = cbLargeKey.SelectedIndex;

            //We tmp remove PropertyChanged as SelectedIndex would become -1 and the client will freak out
            Settings.Current.MultiRPC.PropertyChanged -= MultiRPC_PropertyChanged;
            cbSmallKey.ItemsSource = Data.MultiRPCImages;
            cbLargeKey.ItemsSource = cbSmallKey.ItemsSource;
            cbSmallKey.SelectedIndex = smallInt;
            cbLargeKey.SelectedIndex = largeInt;
            Settings.Current.MultiRPC.PropertyChanged += MultiRPC_PropertyChanged;
        }

        private void MultiRPC_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RPCPreview.UpdateText(defaultSettings: Settings.Current.MultiRPC);
            Rpc.RichPresence = Settings.Current.MultiRPC.ToRichPresence();
        }

        public void UpdateText() 
        {
            tblText1.Text = LanguagePicker.GetLineFromLanguageFile("Text1") + ":";
            tblText2.Text = LanguagePicker.GetLineFromLanguageFile("Text2") + ":";
            tblLargeKey.Text = LanguagePicker.GetLineFromLanguageFile("LargeKey") + ":";
            tblLargeText.Text = LanguagePicker.GetLineFromLanguageFile("LargeText") + ":";
            tblSmallKey.Text = LanguagePicker.GetLineFromLanguageFile("SmallKey") + ":";
            tblSmallText.Text = LanguagePicker.GetLineFromLanguageFile("SmallText") + ":";
            tblElapasedTime.Text = LanguagePicker.GetLineFromLanguageFile("ShowElapsedTime") + ":";
            tblWhatWillLookLike.Text = LanguagePicker.GetLineFromLanguageFile("WhatItWillLookLike");
        }

        public void UpdateStartButtonText()
        {
            if (Rpc.HasConnection)
            {
                return;
            }

            App.Current.Resources["StartButtonText"] =
                LanguagePicker.GetLineFromLanguageFile("Start") + " MultiRPC";
            App.Current.Resources["LastRpcStartButtonText"] =
                LanguagePicker.GetLineFromLanguageFile("Start") + " MultiRPC";
        }

        private void TbText1_OnTextChanged(object sender, TextChangedEventArgs e) =>
           Settings.Current.MultiRPC.Text1 = tbText1.Text;

        private void TbText2_OnTextChanged(object sender, TextChangedEventArgs e) =>
            Settings.Current.MultiRPC.Text2 = tbText2.Text;

        private void CbLargeKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Settings.Current.MultiRPC.LargeKey = cbLargeKey.SelectedIndex;

        private void TbLargeText_OnTextChanged(object sender, TextChangedEventArgs e) =>
            Settings.Current.MultiRPC.LargeText = tbLargeText.Text;

        private void CbSmallKey_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Settings.Current.MultiRPC.SmallKey = cbSmallKey.SelectedIndex;

        private void TbSmallText_OnTextChanged(object sender, TextChangedEventArgs e) =>
            Settings.Current.MultiRPC.SmallText = tbSmallText.Text;

        private void CbElapasedTime_OnCheckedChanged(object sender, RoutedEventArgs e) =>
            Settings.Current.MultiRPC.ShowTime = cbElapasedTime.IsChecked.GetValueOrDefault();

        private void CheckIfRpcCanRun() 
        {
            bool canRun = true;

            if (tbText1.Text.Length == 1)
            {
                tbText1.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbText1.ToolTip = new ToolTip(LanguagePicker.GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong"));
                canRun = false;
            }
            else
            {
                tbText1.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbText1.ToolTip = null;
            }

            if (tbText2.Text.Length == 1)
            {
                tbText2.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbText2.ToolTip = new ToolTip(LanguagePicker.GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong"));
                canRun = false;
            }
            else
            {
                tbText2.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbText2.ToolTip = null;
            }

            if (tbSmallText.Text.Length == 1)
            {
                tbSmallText.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbSmallText.ToolTip = new ToolTip(LanguagePicker.GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong"));
                canRun = false;
            }
            else
            {
                tbSmallText.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbSmallText.ToolTip = null;
            }

            if (tbLargeText.Text.Length == 1)
            {
                tbLargeText.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbLargeText.ToolTip = new ToolTip(LanguagePicker.GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong"));
                canRun = false;
            }
            else
            {
                tbLargeText.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbLargeText.ToolTip = null;
            }

            App.Current.Resources["CanRunRpc"] = canRun;
        }
    }
}
