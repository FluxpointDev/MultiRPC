using MultiRPC.Core;
using MultiRPC.Core.Enums;
using MultiRPC.Core.Rpc;

namespace MultiRPC.GUI.CorePages
{
    /// <summary>
    /// Interaction logic for CustomPage.xaml
    /// </summary>
    public partial class CustomPage : PageWithIcon
    {
        public override MultiRPCIcons IconName { get; } = MultiRPCIcons.Custom;
        public override string JsonContent => "Custom";

        //ToDo: Make this page (will also need to add it making the application id when the connection closes)
        public CustomPage()
        {
            InitializeComponent();
            Loaded += (sender, args) => 
            {
                UpdateStartButtonText();
                Settings.Current.LanguageChanged += (sender, args) => UpdateStartButtonText();
            };
            Unloaded += (sender, args) => Settings.Current.LanguageChanged -= (_, __) => UpdateStartButtonText();
        }

        public void UpdateStartButtonText()
        {
            if (Rpc.HasConnection)
            {
                return;
            }

            App.Current.Resources["StartButtonText"] = LanguagePicker.GetLineFromLanguageFile("StartCustom");
            App.Current.Resources["LastRpcStartButtonText"] = LanguagePicker.GetLineFromLanguageFile("StartCustom");
        }
    }
}
