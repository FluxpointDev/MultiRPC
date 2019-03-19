using System.Windows.Controls;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for LogsPage.xaml
    /// </summary>
    public partial class LogsPage : Page
    {
        public LogsPage()
        {
            InitializeComponent();
            DataContext = App.Logging;
        }
    }
}
