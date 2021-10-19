using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Validation;

namespace MultiRPC.UI.Pages.Rpc
{
    public enum ImagesType
    {
        /// <summary>
        /// Use images from the user's RPC and not our images
        /// </summary>
        Custom,
        /// <summary>
        /// Use the images from us
        /// </summary>
        BuiltIn
    }
    
    public partial class BaseRpcControl : UserControl
    {
        public BaseRpcControl() { }

        public RichPresence RichPresence { get; set; }

        public ImagesType ImageType { get; set; }

        public bool GrabID { get; set; }

        public void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            if (GrabID)
            {
                txtClientID.IsVisible = true;
                txtClientID.AddRpcControl(new Language("ClientID"), s =>
                {
                    RichPresence.Presence.Assets.LargeImageKey = s;
                }, s =>
                {
                    if (s.Length != 18)
                    {
                        return new CheckResult(false, Language.GetText("ClientIDMustBe18CharactersLong"));
                    }

                    _ = CheckID(s);
                    return new CheckResult(true);
                });
            }
            
            if (ImageType == ImagesType.Custom)
            {
                cboLargeKey.IsVisible = false;
                cboSmallKey.IsVisible = false;

                txtLargeKey.IsVisible = true;
                txtSmallKey.IsVisible = true;
                txtLargeKey.AddRpcControl(new Language("LargeKey"), s => RichPresence.Presence.Assets.LargeImageKey = s, s => Check(s, 32));
                txtSmallKey.AddRpcControl(new Language("SmallKey"), s => RichPresence.Presence.Assets.SmallImageKey = s, s => Check(s, 32));
            }
            else
            {
                cboLargeKey.Items = Data.MultiRPCImages.Keys;
                cboSmallKey.Items = Data.MultiRPCImages.Keys;
                cboLargeKey.SelectedIndex = 0;
                cboSmallKey.SelectedIndex = 0;
            }

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
                catch (Exception) { }
            }, CheckUrl);
            txtButton1Text.AddRpcControl(new Language("Button1Text"), s => RichPresence.Presence.Buttons[0].Label = s, s => Check(s, 32));

            txtButton2Url.AddRpcControl(new Language("Button2Url"), s =>
            {
                try
                {
                    RichPresence.Presence.Buttons[1].Url = s;
                }
                catch (Exception) { }
            }, CheckUrl);
            txtButton2Text.AddRpcControl(new Language("Button2Text"), s => RichPresence.Presence.Buttons[1].Label = s, s => Check(s, 32));

            ckbElapsedTime.DataContext = new Language("ShowElapsedTime");
        }

        //TODO: Make it so we can disable start button or update presence button

        private async Task CheckID(string s)
        {
            txtClientID.Classes.Remove("error");

            string? error = null;
            if (long.TryParse(s, out var id))
            {
                txtClientID.Classes.Add("checking");
                var (successful, resultMessage) = await IDChecker.Check(id);
                txtClientID.Classes.Remove("checking");
                if (successful)
                {
                    ToolTip.SetTip(txtClientID, null);
                    RichPresence.ID = id;
                    RichPresence.Name = resultMessage!;
                    return;
                }
                error = resultMessage;
            }
            txtClientID.Classes.Add("error");
            error ??= Language.GetText("ClientIDIsNotValid");
            ToolTip.SetTip(txtClientID, error);
        }
        
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
            var key = e.AddedItems[0]?.ToString();
            RichPresence.Presence.Assets.LargeImageKey = cboLargeKey.SelectedIndex != 0 ? 
                key?.ToLower() : string.Empty;

            RichPresence.CustomLargeImageUrl =
                key != null 
                && Data.TryGetImageValue(key, out var uriS)
                && Uri.TryCreate(uriS, UriKind.Absolute, out var uri)
                    ? uri 
                    : null;
        }

        private void CboSmallKey_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var key = e.AddedItems[0]?.ToString();
            RichPresence.Presence.Assets.SmallImageKey = cboSmallKey.SelectedIndex != 0 ? 
                key?.ToLower() : string.Empty;
            
            RichPresence.CustomSmallImageUrl =
                key != null 
                && Data.TryGetImageValue(key, out var uriS) 
                && Uri.TryCreate(uriS, UriKind.Absolute, out var uri) 
                    ? uri 
                    : null;
        }

        private void CkbElapsedTime_OnChange(object? sender, RoutedEventArgs e)
        {
            RichPresence.UseTimestamp = ckbElapsedTime.IsChecked.GetValueOrDefault(false);
        }
    }
}