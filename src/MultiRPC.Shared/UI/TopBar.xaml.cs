using MultiRPC.Shared.UI.Views;
using static MultiRPC.Core.LanguagePicker;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MultiRPC.Shared.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopBar : LocalizablePage
    {
        //TODO: Move this to RPC client
        ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

        public TopBar()
        {
            InitializeComponent();
            rpcPreview.Content = new RPCView { CurrentView = RPCView.ViewType.Default };
        }

        public override void UpdateText()
        {
            btnStart.Content = $"{GetLineFromLanguageFile("Start")} {GetLineFromLanguageFile("MultiRPC")}";
            btnUpdatePresence.Content = GetLineFromLanguageFile("UpdatePresence");

            rStatus.Text = GetLineFromLanguageFile("Status") + ": ";
            rConnectStatus.Text = GetLineFromLanguageFile(ConnectionStatus.ToString());
            rUser.Text = GetLineFromLanguageFile("User") + ": ";
            rUsername.Text = "Azy#0000"; //TODO: Store and get username
            tblAfk.Text = GetLineFromLanguageFile("AfkText") + ": ";

            btnAuto.Content = GetLineFromLanguageFile("Auto");
            btnAfk.Content = GetLineFromLanguageFile("Afk");
        }
    }

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
    }
}
