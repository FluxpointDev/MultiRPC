using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages
{
    public partial class MultiRpcPage : RpcPage
    {
        public MultiRpcPage()
        {
            InitializeComponent();

            cboLargeKey.Items = new[] { new Language("LargeKey").Text };
            cboSmallKey.Items = new[] { new Language("SmallKey").Text };
            
            txtText1.DataContext = new Language("Text1");
            txtText2.DataContext = new Language("Text2");

            txtLargeText.DataContext = new Language("LargeText");
            txtSmallText.DataContext = new Language("SmallText");
            txtButton1Url.DataContext = new Language("Button1Url");
            txtButton1Text.DataContext = new Language("Button1Text");
            txtButton2Url.DataContext = new Language("Button2Url");
            txtButton2Text.DataContext = new Language("Button2Text");

            ckbElapsedTime.DataContext = new Language("ShowElapsedTime");
            
            txtText1.AddHandler(TextInputEvent, TxtText1_OnTextInput, RoutingStrategies.Tunnel);
            txtText2.AddHandler(TextInputEvent, TxtText2_OnTextInput, RoutingStrategies.Tunnel);
        }

        public override string IconLocation => "Icons/Discord";
        public override string LocalizableName => "MultiRPC";

        public override RichPresence RichPresence { get; protected set; } = new RichPresence("MultiRPC", Constants.MultiRPCID);

        //TODO: Handle this in a better way
        //TODO: See why this isn't called on removing text
        private void TxtText1_OnTextInput(object? sender, TextInputEventArgs e)
        {
            RichPresence.Presence.Details = txtText1.Text + e.Text;
        }
        
        private void TxtText2_OnTextInput(object? sender, TextInputEventArgs e)
        {
            RichPresence.Presence.State = txtText2.Text + e.Text;
        }
    }
}