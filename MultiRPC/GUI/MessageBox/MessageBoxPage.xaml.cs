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
    /// Interaction logic for MessageBoxPage.xaml
    /// </summary>
    public partial class MessageBoxPage : Page
    {
        private long WindowID;

        public MessageBoxPage(string messageBoxText, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, long windowID)
        {
            InitializeComponent();

            WindowID = windowID;
            butNo.Content = App.Text.No;
            butYes.Content = App.Text.Yes;
            butOk.Content = App.Text.Ok;
            butCancel.Content = App.Text.Cancel;
            tblmessageBoxText.Text = messageBoxText;

            if (messageBoxButton == MessageBoxButton.OK)
            {
                butOk.Visibility = Visibility.Visible;
            }
            if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                butOk.Visibility = Visibility.Visible;
                butCancel.Visibility = Visibility.Visible;
            }
            if (messageBoxButton == MessageBoxButton.YesNo)
            {
                butYes.Visibility = Visibility.Visible;
                butNo.Visibility = Visibility.Visible;
            }
            if (messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                butYes.Visibility = Visibility.Visible;
                butNo.Visibility = Visibility.Visible;
                butCancel.Visibility = Visibility.Visible;
            }

            if ((int)messageBoxImage == 0)
                imgmessageBoxImage.Visibility = Visibility.Collapsed;

            if ((int)messageBoxImage == 64)
                imgmessageBoxImage.Source = (ImageSource)App.Current.Resources["InfoIconDrawingImage"];
            else if ((int)messageBoxImage == 48)
                imgmessageBoxImage.Source = (ImageSource)App.Current.Resources["WarningIconDrawingImage"];
            else if ((int)messageBoxImage == 16)
                imgmessageBoxImage.Source = (ImageSource)App.Current.Resources["AlertIconDrawingImage"];
            else if ((int)messageBoxImage == 32)
                imgmessageBoxImage.Source = (ImageSource)App.Current.Resources["HelpIconDrawingImage"];
        }

        private void ButOk_OnClick(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.CloseWindow(WindowID, MessageBoxResult.OK);
        }

        private void ButYes_OnClick(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.CloseWindow(WindowID, MessageBoxResult.Yes);
        }

        private void ButNo_OnClick(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.CloseWindow(WindowID, MessageBoxResult.No);
        }

        private void ButCancel_OnClick(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.CloseWindow(WindowID, MessageBoxResult.Cancel);
        }
    }
}
