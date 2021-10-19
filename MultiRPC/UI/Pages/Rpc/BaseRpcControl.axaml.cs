using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Extensions;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages.Rpc
{
    public partial class BaseRpcControl : UserControl
    {
        public BaseRpcControl() { }

        public RichPresence RichPresence { get; set; }

        public void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            //Note that when it comes to doing the custom page, what theses fill in can't be more then 32bytes
            cboLargeKey.Items = Data.MultiRPCImages.Keys;
            cboSmallKey.Items = Data.MultiRPCImages.Keys;
            cboLargeKey.SelectedIndex = 0;
            cboSmallKey.SelectedIndex = 0;

            txtText1.AddRpcControl(new Language("Text1"), s => RichPresence.Presence.Details = s, s => Check(s));
            txtText2.AddRpcControl(new Language("Text2"), s => RichPresence.Presence.State = s, s => Check(s));
            txtLargeText.AddRpcControl(new Language("LargeText"), s => RichPresence.Presence.Assets.LargeImageText = s, s => Check(s));
            txtSmallText.AddRpcControl(new Language("SmallText"), s => RichPresence.Presence.Assets.SmallImageText = s, s => Check(s));

            //We have to add the try/catch due to the RPC lib not throwing on a empty string (but it still sets url so we good)
            txtButton1Url.AddRpcControl(new Language("Button1Url"), s =>
            {
                try
                {
                    RichPresence.Presence.Buttons[0].Url = s;
                }
                catch (Exception _) { }
            }, CheckUrl);
            txtButton1Text.AddRpcControl(new Language("Button1Text"), s => RichPresence.Presence.Buttons[0].Label = s, s => Check(s, 32));

            txtButton2Url.AddRpcControl(new Language("Button2Url"), s =>
            {
                try
                {
                    RichPresence.Presence.Buttons[1].Url = s;
                }
                catch (Exception _) { }
            }, CheckUrl);
            txtButton2Text.AddRpcControl(new Language("Button2Text"), s => RichPresence.Presence.Buttons[1].Label = s, s => Check(s, 32));

            ckbElapsedTime.DataContext = new Language("ShowElapsedTime");
        }

        //TODO: Make it so we can disable start button or update presence button
        
        private CheckResult CheckUrl(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || Uri.TryCreate(s, UriKind.Absolute, out _))
            {
                return s.CheckBytes(512)
                    ? new CheckResult(true)
                    : new CheckResult(false, Language.GetText("UriTooBig"));
            }

            return new CheckResult(false, Language.GetText("InvalidUri"));
        }

        private static CheckResult Check(string s, int max = 128)
        {
            if (s.Length == 1)
            {
                return new CheckResult(false, Language.GetText("OneChar"));
            }

            return s.CheckBytes(max)
                ? new CheckResult(true)
                : new CheckResult(false, Language.GetText("TooManyChars"));
        }

        private void CboLargeKey_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RichPresence.Presence.Assets.LargeImageKey = cboLargeKey.SelectedIndex != 0 ? 
                e.AddedItems[0]?.ToString()?.ToLower() : string.Empty;
        }

        private void CboSmallKey_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RichPresence.Presence.Assets.SmallImageKey = cboSmallKey.SelectedIndex != 0 ? 
                e.AddedItems[0]?.ToString()?.ToLower() : string.Empty;
        }

        private void CkbElapsedTime_OnChange(object? sender, RoutedEventArgs e)
        {
            RichPresence.UseTimestamp = ckbElapsedTime.IsChecked.GetValueOrDefault(false);
        }
    }
}