using Microsoft.UI.Xaml;
using MultiRPC.Core.Rpc;
using MultiRPC.Shared.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiRPC
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow(MainPage mainPage)
        {
            this.InitializeComponent();
            Content = mainPage;
            RpcPageManager.Load();
        }
    }
}
