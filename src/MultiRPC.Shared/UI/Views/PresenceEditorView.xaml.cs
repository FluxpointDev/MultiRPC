using MultiRPC.Core.Rpc;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using MultiRPC.Core;
using Microsoft.UI.Xaml;
using static MultiRPC.Core.LanguagePicker;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace MultiRPC.Shared.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PresenceEditorView : LocalizablePage
    {
        private Brush DefaultBorder = null!;
        public RichPresence RichPresence { get; }

        public PresenceEditorView(ref RichPresence richPresence, bool isfromCustomPage)
        {
            DataContext = this;
            RichPresence = richPresence;
            this.InitializeComponent();
            if (isfromCustomPage)
            {

            }
        }

        //TODO: Make it so we don't need to do it this way because its so bad
        private void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DefaultBorder ??= txtText1.BorderBrush; //TODO: Find a better way
            var redBorder = Application.Current.Resources["Red"];

            switch (e.PropertyName)
            {
                case nameof(RichPresence.Presence.Details):
                    txtText1.SetValue(BorderBrushProperty, txtText1.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText1, txtText1.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.State):
                    txtText2.SetValue(BorderBrushProperty, txtText2.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtText2, txtText2.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.Assets.LargeImageText):
                    txtLargeText.SetValue(BorderBrushProperty, txtLargeText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtLargeText, txtLargeText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
                case nameof(RichPresence.Presence.Assets.SmallImageText):
                    txtSmallText.SetValue(BorderBrushProperty, txtSmallText.Text.Length == 1 ? redBorder : DefaultBorder);
                    ToolTipService.SetToolTip(txtSmallText, txtSmallText.Text.Length == 1 ? GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong") : null);
                    break;
            }
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

            //Get the MultiRPCImages, need to temp store where we was as we will lose
            //the index on changing it
            var oldLargeIndex = cboLargeKey.SelectedIndex;
            var oldSmallIndex = cboSmallKey.SelectedIndex;
            cboLargeKey.ItemsSource = Data.MultiRPCImages.Keys;
            cboSmallKey.ItemsSource = Data.MultiRPCImages.Keys;
            cboLargeKey.SelectedIndex = oldLargeIndex;
            cboSmallKey.SelectedIndex = oldSmallIndex;
        }

        public void cboLargeKey_SelectionChanged(object sender, RoutedEventArgs args)
        {
            var key = cboLargeKey.SelectedValue.ToString();
            if (!string.IsNullOrWhiteSpace(key))
            {
                RichPresence.Presence.Assets.LargeImageKey = key.ToLower();
                RichPresence.CustomLargeImageUrl = Data.GetImageValue(key);
            }
        }

        public void cboSmallKey_SelectionChanged(object sender, RoutedEventArgs args)
        {
            var key = cboSmallKey.SelectedValue.ToString();
            if (!string.IsNullOrWhiteSpace(key))
            {
                RichPresence.Presence.Assets.SmallImageKey = key.ToLower();
                RichPresence.CustomSmallImageUrl = Data.GetImageValue(key);
            }
        }
    }
}
