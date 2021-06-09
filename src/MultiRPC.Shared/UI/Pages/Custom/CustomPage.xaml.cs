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
        private Brush DefaultBorder;

        public CustomPage() : base(true)
        {
            this.InitializeComponent();
            Loaded += (sender, _) => Accessed?.Invoke(sender, null);
        }

        public event EventHandler Accessed;
        public event PropertyChangedEventHandler PropertyChanged;

        public override async void UpdateText()
        {
            tblID.Text = $"{ GetLineFromLanguageFile("ClientID")}:";
            tblText1.Text = $"{GetLineFromLanguageFile("Text1")}:";
            tblText2.Text = $"{GetLineFromLanguageFile("Text2")}:";
            tblLargeKey.Text = $"{GetLineFromLanguageFile("LargeKey")}:";
            tblLargeText.Text = $"{GetLineFromLanguageFile("LargeText")}:";
            tblSmallKey.Text = $"{GetLineFromLanguageFile("SmallKey")}:";
            tblSmallText.Text = $"{GetLineFromLanguageFile("SmallText")}:";
            tblElapasedTime.Text = $"{GetLineFromLanguageFile("ShowElapsedTime")}:";
        }

        private async void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DefaultBorder ??= txtText1.BorderBrush; //TODO: Find a better way
            var redBorder = Application.Current.Resources["Red"];

            switch (e.PropertyName)
            {
                case nameof(RichPresence.Details):
                    txtText1.SetValue(BorderBrushProperty, txtText1.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText1, txtText1.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.State):
                    txtText2.SetValue(BorderBrushProperty, txtText2.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText2, txtText2.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Assets.LargeImage.Text): //Check Small + Large
                    txtLargeText.SetValue(BorderBrushProperty, txtLargeText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtLargeText, txtLargeText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);

                    txtSmallText.SetValue(BorderBrushProperty, txtSmallText.Text.Length == 1 ? redBorder : DefaultBorder);
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
            RichPresence.Details = txtText1.Text;

        public void txtText2_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.State = txtText2.Text;

        public void txtSmallText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.SmallImage.Text = txtSmallText.Text;

        public void txtLargeText_TextChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.LargeImage.Text = txtLargeText.Text;

        public void txtLargeKey_SelectionChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.LargeImage.Key = txtLargeKey.Text;

        public void txtSmallKey_SelectionChanged(object sender, TextChangedEventArgs args) =>
            RichPresence.Assets.SmallImage.Key = txtSmallKey.Text;

        public void cbElapasedTime_CheckedChanged(object sender, RoutedEventArgs args)
        {
            RichPresence.Timestamp.Start = null;
            RichPresence.Timestamp.SetStartOnRPCConnection =
                cbElapasedTime.IsChecked.GetValueOrDefault();
        }

        private async void txtID_TextChanged(object sender, TextChangedEventArgs e)
        {
            DefaultBorder ??= txtText1.BorderBrush; //TODO: Find a better way

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
                //Remake RichPresence with new ID + Name
                if (RichPresence != null)
                {
                    RichPresence.PropertyChanged -= RichPresence_PropertyChanged;
                    RichPresence.Assets.LargeImage.PropertyChanged -= RichPresence_PropertyChanged;
                    RichPresence.Assets.SmallImage.PropertyChanged -= RichPresence_PropertyChanged;
                }
                RichPresence = RichPresence.Clone(checkResult.resultMessage, id);

                RichPresence.PropertyChanged += RichPresence_PropertyChanged;
                RichPresence.Assets.LargeImage.PropertyChanged += RichPresence_PropertyChanged;
                RichPresence.Assets.SmallImage.PropertyChanged += RichPresence_PropertyChanged;

                //Check everything manally as we might have not checked yet
                RichPresence_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(RichPresence.Details)));
                RichPresence_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(RichPresence.State)));
                RichPresence_PropertyChanged(sender, new PropertyChangedEventArgs(nameof(RichPresence.Assets.LargeImage.Text)));

                //Show default things
                txtID.SetValue(BorderBrushProperty, DefaultBorder);
                ToolTipService.SetToolTip(txtID, checkResult.resultMessage);

                LastIDCheckStatus = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VaildRichPresence)));
                return;
            }

            //If we got here then something failed when checking, show that's the case
            txtID.SetValue(BorderBrushProperty, Application.Current.Resources["Red"]);
            ToolTipService.SetToolTip(txtID, checkResult.resultMessage);
        }
    }
}
