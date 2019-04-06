using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

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
            this.Name = "win" + DateTime.Now.Ticks;
            this.RegisterName(this.Name, this);

            WindowID = windowID;
            btnNo.Content = App.Text.No;
            btnYes.Content = App.Text.Yes;
            btnOk.Content = App.Text.Ok;
            btnCancel.Content = App.Text.Cancel;
            tblMessageBoxText.Text = messageBoxText;

            if (messageBoxButton == MessageBoxButton.OK)
            {
                btnOk.Visibility = Visibility.Visible;
            }
            if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                btnOk.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
            }
            if (messageBoxButton == MessageBoxButton.YesNo)
            {
                btnYes.Visibility = Visibility.Visible;
                btnNo.Visibility = Visibility.Visible;
            }
            if (messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                btnYes.Visibility = Visibility.Visible;
                btnNo.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
            }

            if ((int)messageBoxImage == 0)
                imgMessageBoxImage.Visibility = Visibility.Collapsed;

            if ((int)messageBoxImage == 64)
                imgMessageBoxImage.Source = (ImageSource)App.Current.Resources["InfoIconDrawingImage"];
            else if ((int)messageBoxImage == 48)
                imgMessageBoxImage.Source = (ImageSource)App.Current.Resources["WarningIconDrawingImage"];
            else if ((int)messageBoxImage == 16)
                imgMessageBoxImage.Source = (ImageSource)App.Current.Resources["AlertIconDrawingImage"];
            else if ((int)messageBoxImage == 32)
                imgMessageBoxImage.Source = (ImageSource)App.Current.Resources["HelpIconDrawingImage"];
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
