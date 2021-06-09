using MultiRPC.Core.Rpc;
using System;
using static MultiRPC.Core.LanguagePicker;
using System.ComponentModel;
using MultiRPC.Core.Page;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MultiRPC.Shared.UI.Pages.Custom
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomPage : TabbedPage, IRpcPage
    {
        public CustomPage() : base(true)
        {
            this.InitializeComponent();
            Loaded += (sender, _) => Accessed?.Invoke(sender, null);
        }

        public event EventHandler Accessed;
        public event PropertyChangedEventHandler PropertyChanged;

        public override void UpdateText()
        {
            tblID.Text = $"{GetLineFromLanguageFile("ClientID")}:";
            tblText1.Text = $"{GetLineFromLanguageFile("Text1")}:";
            tblText2.Text = $"{GetLineFromLanguageFile("Text2")}:";
            tblLargeKey.Text = $"{GetLineFromLanguageFile("LargeKey")}:";
            tblLargeText.Text = $"{GetLineFromLanguageFile("LargeText")}:";
            tblSmallKey.Text = $"{GetLineFromLanguageFile("SmallKey")}:";
            tblSmallText.Text = $"{GetLineFromLanguageFile("SmallText")}:";
            tblElapasedTime.Text = $"{GetLineFromLanguageFile("ShowElapsedTime")}:";
        }

        private void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var redBorder = Application.Current.Resources["Red"];

            switch (e.PropertyName)
            {
                case nameof(RichPresence.Presence.Details):
                    txtText1.SetValue(BorderBrushProperty, txtText1.Text.Length == 1 ? redBorder : null);
                    ToolTipService.SetToolTip(txtText1, txtText1.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.State):
                    txtText2.SetValue(BorderBrushProperty, txtText2.Text.Length == 1 ? redBorder : null);
                    ToolTipService.SetToolTip(txtText2, txtText2.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.Assets.LargeImageText): //Check Small + Large
                    txtLargeText.SetValue(BorderBrushProperty, txtLargeText.Text.Length == 1 ? redBorder : null);
                    ToolTipService.SetToolTip(txtLargeText, txtLargeText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);

                    txtSmallText.SetValue(BorderBrushProperty, txtSmallText.Text.Length == 1 ? redBorder : null);
                    ToolTipService.SetToolTip(txtSmallText, txtSmallText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
            }

            PropertyChanged?.Invoke(this, e);
        }

        public string IconLocation => "Icon/Page/Custom";

        public string LocalizableName => "Custom";

        public RichPresence RichPresence { get; private set; } = new RichPresence("", 0);

        public bool VaildRichPresence => LastIDCheckStatus && txtText1.Text.Length != 1 && txtText2.Text.Length != 1 && txtLargeText.Text.Length != 1 && txtSmallText.Text.Length != 1;

        private bool LastIDCheckStatus;


        public void txtText1_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Details = txtText1.Text;

        public void txtText2_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.State = txtText2.Text;

        public void txtSmallText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Assets.SmallImageText = txtSmallText.Text;

        public void txtLargeText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Assets.LargeImageText = txtLargeText.Text;

        public void txtLargeKey_SelectionChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Assets.LargeImageKey = txtLargeKey.Text;

        public void txtSmallKey_SelectionChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Presence.Assets.SmallImageKey = txtSmallKey.Text;

        public void cbElapasedTime_CheckedChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Presence.Timestamps.Start = 
                cbElapasedTime.IsChecked.GetValueOrDefault() 
                    ? DateTime.MinValue 
                    : null;
        }

        private async void txtID_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Check that the id given is something that discord accepts
            var isValidCode =
                long.TryParse(txtID.Text, NumberStyles.Any, new NumberFormatInfo(), out var id);

            LastIDCheckStatus = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VaildRichPresence)));

            if (!isValidCode || txtID.Text.Length != 18)
            {
                txtID.SetValue(BorderBrushProperty, Application.Current.Resources["Red"]);
                ToolTipService.SetToolTip(txtID, !isValidCode
                    ? GetLineFromLanguageFile("ClientIDIsNotValid")
                    : GetLineFromLanguageFile("ClientIDMustBe18CharactersLong"));

                return;
            }

            //Show that we are checking
            txtID.SetValue(BorderBrushProperty, Application.Current.Resources["Orange"]);
            ToolTipService.SetToolTip(txtSmallText, GetLineFromLanguageFile("CheckingClientID"));

            //Change textbox based on if we got success or not
            var checkResult = await ClientIDChecker.CheckID(id);
            if (checkResult.Successful)
            {
                RichPresence = new RichPresence(checkResult.ResultMessage, id)
                {
                    Presence = RichPresence.Presence
                };

                //Check everything manually as we might have not checked yet
                RichPresence_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(RichPresence.Presence.Details)));
                RichPresence_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(RichPresence.Presence.State)));
                RichPresence_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(RichPresence.Presence.Assets.LargeImageText)));

                //Show default things
                txtID.SetValue(BorderBrushProperty, null);
                ToolTipService.SetToolTip(txtID, checkResult.ResultMessage);

                LastIDCheckStatus = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VaildRichPresence)));
                return;
            }

            //If we got here then something failed when checking, show that's the case
            txtID.SetValue(BorderBrushProperty, Application.Current.Resources["Red"]);
            ToolTipService.SetToolTip(txtID, checkResult.ResultMessage);
        }
    }
}
