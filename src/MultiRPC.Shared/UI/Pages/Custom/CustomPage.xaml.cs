using MultiRPC.Core.Rpc;
using System;
using static MultiRPC.Core.LanguagePicker;
using System.ComponentModel;
using MultiRPC.Core.Pages;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MultiRPC.Shared.UI.Views;

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
            cclEditor.Content = new PresenceEditorView(ref _richPresence, true);
            Loaded += (sender, _) => Accessed?.Invoke(sender, null);
        }

        public event EventHandler Accessed;
        public event PropertyChangedEventHandler PropertyChanged;

        public override void UpdateText()
        {
            tblID.Text = $"{GetLineFromLanguageFile("ClientID")}:";
        }

        public string IconLocation => "Icon/Page/Custom";

        public string LocalizableName => "Custom";

        private RichPresence _richPresence = new RichPresence("", 0);
        public RichPresence RichPresence => _richPresence;

        public bool VaildRichPresence => LastIDCheckStatus && RichPresence.IsVaildPresence;

        private bool LastIDCheckStatus;

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
            ToolTipService.SetToolTip(txtID, GetLineFromLanguageFile("CheckingClientID"));

            //Change textbox based on if we got success or not
            var checkResult = await ClientIDChecker.CheckID(id);
            if (checkResult.Successful)
            {
                _richPresence = new RichPresence(checkResult.ResultMessage, id)
                {
                    Presence = RichPresence.Presence
                };

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
