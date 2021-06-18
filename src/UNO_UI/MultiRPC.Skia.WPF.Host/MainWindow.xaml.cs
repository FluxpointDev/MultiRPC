using MultiRPC.Core.Rpc;
using MultiRPC.Uno;
using System.Windows;
using Uno.UI.Skia.Platform;

namespace MultiRPC.WPF.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            root.Content = new WpfHost(Dispatcher, () => new UnoApp());
            RpcPageManager.Load();
        }
    }
}
