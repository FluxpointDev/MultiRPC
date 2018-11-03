using System.Windows;
using System.Windows.Controls;

namespace MultiRPC
{
    /// <summary>
    /// Interaction logic for ProgramSettings.xaml
    /// </summary>
    public partial class ProgramSettings : UserControl
    {
        public ProgramSettings()
        {
            InitializeComponent();
        }

        public string Type;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IProgram Prog = Data.Programs[Type];
            Prog.Data.Enabled = true;
            Prog.Data.Save();
        }

        private void TextBox_details_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(Text_Priority.Text, out int Number))
            {
                IProgram Prog = Data.Programs[Type];
                Prog.Data.Priority = Number;
                Prog.Data.Save();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            IProgram Prog = Data.Programs[Type];
            Prog.Data.Enabled = false;
            Prog.Data.Save();
        }
    }
}
