using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for ProgramsPage.xaml
    /// </summary>
    public partial class ProgramsPage : Page
    {
        public static ProgramsPage _ProgramsPage;

        public ProgramsPage()
        {
            InitializeComponent();
            _ProgramsPage = this;
        }

        public Task UpdateText()
        {
            return Task.CompletedTask;
        }
    }
}