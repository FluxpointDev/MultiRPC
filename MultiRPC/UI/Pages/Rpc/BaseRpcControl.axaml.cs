using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Validation;
using MultiRPC.UI.Controls;
using TinyUpdate.Core.Extensions;

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
    
    public partial class BaseRpcControl : UserControl, ITabPage
    {
        public RichPresence RichPresence { get; set; } = null!;
        public ImagesType ImageType { get; set; }
        public bool GrabID { get; set; }
        public Language TabName { get; init; }

        public bool IsDefaultPage => true;

        public void ChangeRichPresence(RichPresence richPresence)
        {
            RichPresence = richPresence;

            txtClientID.Text = richPresence.ID.ToString();
            txtText1.Text = richPresence.Profile.Details;
            txtText2.Text = richPresence.Profile.State;
            txtLargeKey.Text = richPresence.Profile.LargeKey;
            txtLargeText.Text = richPresence.Profile.LargeText;
            txtSmallKey.Text = richPresence.Profile.SmallKey;
            txtSmallText.Text = richPresence.Profile.SmallText;
            txtButton1Url.Text = richPresence.Profile.Button1Url;
            txtButton1Text.Text = richPresence.Profile.Button1Text;
            txtButton2Url.Text = richPresence.Profile.Button2Url;
            txtButton2Text.Text = richPresence.Profile.Button2Text;
            ckbElapsedTime.IsChecked = richPresence.Profile.ShowTime;
        }

        public void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            if (GrabID)
            {
                txtClientID.IsVisible = true;
                txtClientID.AddRpcControl(new Language("ClientID"), null, s =>
                {
                    if (s.Length != 18)
                    {
                        return new CheckResult(false, Language.GetText("ClientIDMustBe18CharactersLong"));
                    }

                    _ = CheckID(s);
                    return new CheckResult(true);
                }, RichPresence.ID.ToString());
            }
            
            if (ImageType == ImagesType.Custom)
            {
                cboLargeKey.IsVisible = false;
                cboSmallKey.IsVisible = false;

                txtLargeKey.IsVisible = true;
                txtSmallKey.IsVisible = true;
                txtLargeKey.AddRpcControl(new Language("LargeKey"), s => RichPresence.Profile.LargeKey = s, s => Check(s, 32), RichPresence.Profile.LargeKey);
                txtSmallKey.AddRpcControl(new Language("SmallKey"), s => RichPresence.Profile.SmallKey = s, s => Check(s, 32), RichPresence.Profile.SmallKey);
            }
            else
            {
                cboLargeKey.Items = Data.MultiRPCImages.Keys;
                cboSmallKey.Items = Data.MultiRPCImages.Keys;
                var largeKey = Data.MultiRPCImages.Keys.IndexOf(x => x?.ToLower() == RichPresence.Profile.LargeKey);
                if (largeKey == -1)
                {
                    largeKey = 0;
                }
                cboLargeKey.SelectedIndex = largeKey;
                
                var smallKey = Data.MultiRPCImages.Keys.IndexOf(x => x?.ToLower() == RichPresence.Profile.SmallKey);
                if (smallKey == -1)
                {
                    smallKey = 0;
                }
                cboSmallKey.SelectedIndex = smallKey;
            }

            txtText1.AddRpcControl(new Language("Text1"), s => RichPresence.Profile.Details = s, s => Check(s), RichPresence.Profile.Details);
            txtText2.AddRpcControl(new Language("Text2"), s => RichPresence.Profile.State = s, s => Check(s), RichPresence.Profile.State);
            txtLargeText.AddRpcControl(new Language("LargeText"), s => RichPresence.Profile.LargeText = s, s => Check(s), RichPresence.Profile.LargeText);
            txtSmallText.AddRpcControl(new Language("SmallText"), s => RichPresence.Profile.SmallText = s, s => Check(s), RichPresence.Profile.SmallText);

            txtButton1Url.AddRpcControl(new Language("Button1Url"), s => RichPresence.Profile.Button1Url = s, CheckUrl, RichPresence.Profile.Button1Url);
            txtButton1Text.AddRpcControl(new Language("Button1Text"), s => RichPresence.Profile.Button1Text = s, s => Check(s, 32), RichPresence.Profile.Button1Text);
            txtButton2Url.AddRpcControl(new Language("Button2Url"), s => RichPresence.Profile.Button2Url = s, CheckUrl, RichPresence.Profile.Button2Url);
            txtButton2Text.AddRpcControl(new Language("Button2Text"), s => RichPresence.Profile.Button2Text = s, s => Check(s, 32), RichPresence.Profile.Button2Text);

            ckbElapsedTime.IsChecked = RichPresence.UseTimestamp;
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
                    if (RichPresence.Name.StartsWith("Custom"))
                    {
                        RichPresence.Name = resultMessage!;
                    }
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
            RichPresence.Profile.LargeKey = cboLargeKey.SelectedIndex != 0 ? 
                key?.ToLower() ?? string.Empty : string.Empty;

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
            RichPresence.Profile.SmallKey = cboSmallKey.SelectedIndex != 0 ? 
                key?.ToLower() ?? string.Empty : string.Empty;
            
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