using System;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using MultiRPC.Functions;
using MultiRPC.GUI.Views;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPageThumbnail : Page
    {
        public MainPageThumbnail(ResourceDictionary resource)
        {
            InitializeComponent();
            this.Resources.MergedDictionaries.Add(resource);
            this.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(File.ReadAllText($"Assets/Icons.xaml")));
            Loaded += MainPageThumbnail_Loaded;
        }

        private void MainPageThumbnail_Loaded(object sender, RoutedEventArgs e)
        {
            DrawingCollection ButtonDrawing(Button btn)
            {
                return ((DrawingGroup) ((DrawingImage) ((Image) btn.Content).Source).Drawing).Children;
            }

            void UpdateButton(Button btn, SolidColorBrush brushToUpdateTo)
            {
                foreach (DrawingGroup group in ButtonDrawing(btn))
                {
                    foreach (GeometryDrawing drawing in group.Children)
                    {
                        drawing.Brush = brushToUpdateTo;
                    }
                }
            }

            UpdateButton(btnNavButton, (SolidColorBrush)this.Resources["AccentColour3SCBrush"]);
            UpdateButton(btnNavButtonSelected, new SolidColorBrush(Colors.White));

            //((Image)btnNavButton.Content).Source = (DrawingImage)this.Resources.MergedDictionaries[1][ImageName("DiscordIcon", false)];
            //((Image)btnNavButtonSelected.Content).Source = (DrawingImage)this.Resources.MergedDictionaries[1][ImageName("DiscordIcon", true)];
        }
    }
}
