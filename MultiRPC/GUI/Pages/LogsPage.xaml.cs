using System.Windows.Controls;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for LogsPage.xaml
    /// </summary>
    public partial class LogsPage : Page
    {
        public LogsPage(double mainPageWidth)
        {
            InitializeComponent();
            DataContext = App.Logging;
            tbLogText.MaxWidth = mainPageWidth;
        }
    }
}