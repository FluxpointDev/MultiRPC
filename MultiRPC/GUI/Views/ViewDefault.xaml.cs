using MultiRPC.Data;
using MultiRPC.Functions;
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

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ViewDefault.xaml
    /// </summary>
    public partial class ViewDefault : UserControl
    {
        public ViewDefault(Style style)
        {
            InitializeComponent();
            if (style != null)
            {
                ItemsLarge.Style = style;
                ItemsSmall.Style = style;
            }
            View = new ViewRPC(ViewType.Default2);
            FrameRPC.Content = View;
        }
        private ViewRPC View;

        public void SetData()
        {
            foreach (string s in _Data.MultiRPC_Images.Keys)
            {
                ComboBoxItem Box = new ComboBoxItem
                {
                    Content = s,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Background = new SolidColorBrush(Color.FromRgb(182, 182, 182))
                };
                ComboBoxItem Box2 = new ComboBoxItem
                {
                    Content = s,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Background = new SolidColorBrush(Color.FromRgb(182, 182, 182))
                };
                ItemsLarge.Items.Add(Box);
                ItemsSmall.Items.Add(Box2);
            }
            TextText1.Text = App.Config.MultiRPC.Text1;
            TextText2.Text = App.Config.MultiRPC.Text2;
            TextLarge.Text = App.Config.MultiRPC.LargeText;
            TextSmall.Text = App.Config.MultiRPC.SmallText;
            if (App.Config.MultiRPC.LargeKey != -1)
                ItemsLarge.SelectedItem = ItemsLarge.Items[App.Config.MultiRPC.LargeKey];
            if (App.Config.MultiRPC.SmallKey != -1)
               ItemsSmall.SelectedItem = ItemsSmall.Items[App.Config.MultiRPC.SmallKey];
        }

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!App.FormReady)
                return;
            ComboBox Box = sender as ComboBox;
            if (Box.SelectedIndex == -1)
                return;
            if (Box.Name == "ItemsLarge")
            {
                App.Config.MultiRPC.LargeKey = Box.SelectedIndex;
                if (Box.SelectedIndex == 0)
                    View.LargeImage.Visibility = Visibility.Hidden;
                else
                {
                    BitmapImage Large = new BitmapImage(new Uri(_Data.MultiRPC_Images[(Box.SelectedItem as ComboBoxItem).Content.ToString()]));
                    Large.DownloadFailed += ViewRPC.Image_FailedLoading;
                    View.LargeImage.Visibility = Visibility.Visible;
                    View.LargeImage.Source = Large;
                }
            }
            else
            {
                App.Config.MultiRPC.SmallKey = Box.SelectedIndex;
                if (Box.SelectedIndex == 0)
                {
                    View.SmallBack.Visibility = Visibility.Hidden;
                    View.SmallImage.Visibility = Visibility.Hidden;
                }
                else
                {
                    BitmapImage Small = new BitmapImage(new Uri(_Data.MultiRPC_Images[(Box.SelectedItem as ComboBoxItem).Content.ToString()]));
                    Small.DownloadFailed += ViewRPC.Image_FailedLoading;
                    View.SmallBack.Visibility = Visibility.Visible;
                    View.SmallImage.Visibility = Visibility.Visible;
                    View.SmallImage.Fill = new ImageBrush(Small);
                }
            }
        }

        private void Default_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Box = sender as TextBox;
            switch (Box.Name)
            {
                case "TextText1":
                    View.Text1.Content = Box.Text;
                    if (App.FormReady)
                        App.Config.MultiRPC.Text1 = Box.Text;
                    break;
                case "TextText2":
                    View.Text2.Content = Box.Text;
                    if (App.FormReady)
                        App.Config.MultiRPC.Text2 = Box.Text;
                    break;
                case "TextLarge":
                    if (string.IsNullOrEmpty(Box.Text))
                        View.LargeImage.ToolTip = null;
                    else
                        View.LargeImage.ToolTip = new Button().Content = Box.Text;
                    if (App.FormReady)
                        App.Config.MultiRPC.LargeText = Box.Text;
                    break;
                case "TextSmall":
                    if (string.IsNullOrEmpty(Box.Text))
                        View.SmallImage.ToolTip = null;
                    else
                        View.SmallImage.ToolTip = new Button().Content = Box.Text;
                    if (App.FormReady)
                        App.Config.MultiRPC.SmallText = Box.Text;
                    break;
            }
            SetLimitNumber(Box);
            if (Box.Text.Length == 25)
                Box.Opacity = 0.80;
            else
                Box.Opacity = 1;
        }

        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            SetLimitVisibility(box, Visibility.Visible);
        }

        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            SetLimitVisibility(box, Visibility.Hidden);
        }

        private void SetLimitVisibility(TextBox box, Visibility vis)
        {
            switch (box.Name)
            {
                case "TextDefaultText1":
                    LimitText1.Visibility = vis;
                    break;
                case "TextDefaultText2":
                    LimitText2.Visibility = vis;
                    break;
                case "TextDefaultLarge":
                    LimitLargeText.Visibility = vis;
                    break;
                case "TextDefaultSmall":
                    LimitSmallText.Visibility = vis;
                    break;
            }
        }

        private void SetLimitNumber(TextBox box)
        {
            double db = 0.50;
            if (box.Text.Length == 25)
                db = 1;
            else if (box.Text.Length > 20)
                db = 0.90;
            else if (box.Text.Length > 15)
                db = 0.80;
            else if (box.Text.Length > 10)
                db = 0.70;
            else if (box.Text.Length > 5)
                db = 0.60;
            switch (box.Name)
            {
                case "TextDefaultText1":
                    LimitText1.Content = 25 - box.Text.Length;
                    LimitText1.Opacity = db;
                    break;
                case "TextDefaultText2":
                    LimitText2.Content = 25 - box.Text.Length;
                    LimitLargeText.Opacity = db;
                    break;
                case "TextDefaultLarge":
                    LimitLargeText.Content = 25 - box.Text.Length;
                    LimitLargeText.Opacity = db;
                    break;
                case "TextDefaultSmall":
                    LimitSmallText.Content = 25 - box.Text.Length;
                    LimitSmallText.Opacity = db;
                    break;
            }
        }
    }
}
