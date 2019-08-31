using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MultiRPC.Functions;
using MultiRPC.JsonClasses;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPageThumbnail : Page
    {
        public MainPageThumbnail(Theme theme)
        {
            InitializeComponent();
            Resources.MergedDictionaries.Add(Theme.ThemeToResourceDictionary(theme));
            Resources.MergedDictionaries.Add(
                (ResourceDictionary) XamlReader.Parse(File.ReadAllText(Path.Combine("Assets", "Icons.xaml"))));
        }

        public MainPageThumbnail(ResourceDictionary resource)
        {
            InitializeComponent();
            Resources.MergedDictionaries.Add(resource);
            Resources.MergedDictionaries.Add(
                (ResourceDictionary) XamlReader.Parse(File.ReadAllText(Path.Combine("Assets", "Icons.xaml"))));
        }

        public async Task UpdateMergedDictionaries(string solidBrushKey, SolidColorBrush brush, string colourKey = null)
        {
            Resources.MergedDictionaries[0][solidBrushKey] = brush;
            if (!string.IsNullOrWhiteSpace(colourKey))
            {
                Resources.MergedDictionaries[0][colourKey] = brush.Color;
            }

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
                return ((DrawingGroup) ((DrawingImage) ((Image) btn.Content).Source).Drawing).Children;
            }

            void UpdateButtonColour(Button btn, SolidColorBrush brushToUpdateTo)
            {
                var mainButtonDrawings = ButtonDrawing(btn);
                for (var i = 0; i < mainButtonDrawings.Count; i++)
                {
                    var buttonDrawings = (DrawingGroup) mainButtonDrawings[i];
                    for (var j = 0; j < buttonDrawings.Children.Count; j++)
                    {
                        ((GeometryDrawing) buttonDrawings.Children[j]).Brush = brushToUpdateTo;
                    }
                }
            }

            UpdateButtonColour(btnNavButton, (SolidColorBrush) Resources["AccentColour3SCBrush"]);
            UpdateButtonColour(btnNavButtonSelected, (SolidColorBrush) Resources["NavButtonIconColourSelected"]);

            return Task.CompletedTask;
        }

        private void MainPageThumbnail_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtons();
            UpdateText();
        }

        private void ChangePage_OnMouseDown(object sender, MouseEventArgs e)
        {
            var button = (Button) sender;
            Animations.ThicknessAnimation(button, new Thickness(2), button.Margin);
        }

        private void ChangePage_OnMouseUp(object sender, MouseEventArgs e)
        {
            var button = (Button) sender;
            Animations.ThicknessAnimation(button, new Thickness(0), button.Margin, ease: new BounceEase());
        }
    }
}