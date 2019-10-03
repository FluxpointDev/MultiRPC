using System.Threading.Tasks;
using System.Windows.Controls;
using MultiRPC.GUI.Controls;
using TabItem = MultiRPC.GUI.Controls.TabItem;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for MasterThemeEditorPage.xaml
    /// </summary>
    public partial class MasterThemeEditorPage : Page
    {
        public static MasterThemeEditorPage _MasterThemeEditorPage;
        private TabPage _tabPage;

        public MasterThemeEditorPage()
        {
            InitializeComponent();
            _tabPage = new TabPage(new[]
            {
                new TabItem
                {
                    TabName = App.Text.ThemeEditor,
                    Page = new ThemeEditorPage()
                },
                new TabItem
                {
                    TabName = App.Text.InstalledThemes,
                    Page = new InstalledThemes()
                }
            });
            frmContent.Content = _tabPage;
            _MasterThemeEditorPage = this;
        }

        public Task UpdateText()
        {
            _tabPage.UpdateText(App.Text.ThemeEditor, App.Text.InstalledThemes);

            return Task.CompletedTask;
        }
    }
}