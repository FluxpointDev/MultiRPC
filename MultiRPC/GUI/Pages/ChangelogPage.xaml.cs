using System.IO;
using System.Windows.Controls;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for ChangelogPage.xaml
    /// </summary>
    public partial class ChangelogPage : Page
    {
        public ChangelogPage()
        {
            InitializeComponent();
            if (File.Exists(FileLocations.ChangelogFileLocalLocation))
                tbChangelogText.Text = File.ReadAllText(FileLocations.ChangelogFileLocalLocation);

            Title = App.Text.Changelog;
        }
    }
}