using System.IO;
using System.Windows;
using MultiRPC.JsonClasses;
using System.Windows.Media;
using System.Windows.Markup;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPageThumbnail : Page
    {
        public MainPageThumbnail(Theme theme)
        {
            InitializeComponent();
            this.Resources.MergedDictionaries.Add(Theme.ThemeToResourceDictionary(theme));
            this.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));
            Loaded += MainPageThumbnail_Loaded;
        }

        public MainPageThumbnail(ResourceDictionary resource)
        {
            InitializeComponent();
            this.Resources.MergedDictionaries.Add(resource);
            this.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));
            Loaded += MainPageThumbnail_Loaded;
        }

        public async Task UpdateMergedDictionaries(string solidBrushKey, SolidColorBrush brush, string colourKey = null)
        {
            this.Resources.MergedDictionaries[0][solidBrushKey] = brush;
            if (!string.IsNullOrWhiteSpace(colourKey))
                this.Resources.MergedDictionaries[0][colourKey] = brush.Color;

            await UpdateButtons();
        }

        private Task UpdateText()
        {
            tbExample.Text = App.Text.WewTextbox;
            cbiExample.Content = App.Text.WewComboboxItem;
            cbiExample2.Content = App.Text.WewComboboxItem2;
            tblExample.Text = App.Text.WewTextBlock;
            btnExample.Content = App.Text.WewButton;
            btnDisabledExample.Content = App.Text.WewDisabledButton;
            return Task.CompletedTask;
        }

        private Task UpdateButtons()
        {
            DrawingCollection ButtonDrawing(Button btn)
            {
                return ((DrawingGroup)((DrawingImage)((Image)btn.Content).Source).Drawing).Children;
            }

            void UpdateButtonColour(Button btn, SolidColorBrush brushToUpdateTo)
            {
                var mainButtonDrawings = ButtonDrawing(btn);
                for (int i = 0; i < mainButtonDrawings.Count; i++)
                {
                    var buttonDrawings = (DrawingGroup) mainButtonDrawings[i];
                    for (int j = 0; j < buttonDrawings.Children.Count; j++)
                    {
                        ((GeometryDrawing) buttonDrawings.Children[j]).Brush = brushToUpdateTo;
                    }
                }
            }

            UpdateButtonColour(btnNavButton, (SolidColorBrush)this.Resources["AccentColour3SCBrush"]);
            UpdateButtonColour(btnNavButtonSelected, (SolidColorBrush)this.Resources["NavButtonIconColourSelected"]);
            return Task.CompletedTask;
        }

        private void MainPageThumbnail_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtons();
            UpdateText();
        }
    }
}
