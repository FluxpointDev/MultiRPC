using MultiRPC.Core.Enums;
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

namespace MultiRPC.GUI.CorePages
{
    /// <summary>
    /// Interaction logic for ProgramPage.xaml
    /// </summary>
    public partial class ProgramPage : PageWithIcon
    {
        public override MultiRPCIcons IconName => MultiRPCIcons.Programs;
        public override string JsonContent => "Programs";

        public ProgramPage()
        {
            InitializeComponent();
        }
    }
}
