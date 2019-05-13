using System.IO;
using MultiRPC.JsonClasses;
using System.Windows.Controls;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for ChangelogPage.xaml
    /// </summary>
    public partial class ChangelogPage : Page
    {
        public ChangelogPage()
        {
            InitializeComponent();
            if (File.Exists(File.ReadAllText(FileLocations.ChangelogFileLocalLocation)))
                tbChangelogText.Text = File.ReadAllText(FileLocations.ChangelogFileLocalLocation);

            Title = App.Text.Changelog;
        }
    }
}
