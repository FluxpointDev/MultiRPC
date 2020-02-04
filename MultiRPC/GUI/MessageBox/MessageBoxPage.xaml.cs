using System;
using System.Windows;
using System.Windows.Controls;
using MultiRPC.Core;

namespace MultiRPC.GUI.MessageBox
{
    /// <summary>
    /// Interaction logic for MessageBoxPage.xaml
    /// </summary>
    public partial class MessageBoxPage : Page
    {
        private CustomMessageBox CustomMessageBox;

        public MessageBoxPage(string messageBoxText, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, CustomMessageBox customMessageBox)
        {
            InitializeComponent();

            CustomMessageBox = customMessageBox;

            btnNo.Content = LanguagePicker.GetLineFromLanguageFile("No");
            btnYes.Content = LanguagePicker.GetLineFromLanguageFile("Yes");
            btnOk.Content = LanguagePicker.GetLineFromLanguageFile("Ok");
            btnCancel.Content = LanguagePicker.GetLineFromLanguageFile("Cancel");
            tblMessageBoxText.Text = messageBoxText;

            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    btnOk.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    btnOk.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageBoxButton), messageBoxButton, null);
            }

            switch ((int) messageBoxImage)
            {
                case 0:
                    imgMessageBoxImage.Visibility = Visibility.Collapsed;
                    break;
                case 64:
                    imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "InfoIconDrawingImage");
                    break;
                case 48:
                    imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "WarningIconDrawingImage");
                    break;
                case 16:
                    imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "AlertIconDrawingImage");
                    break;
                case 32:
                    imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "HelpIconDrawingImage");
                    break;
            }
        }

        private void ButOk_OnClick(object sender, RoutedEventArgs e)
        {
            CloseWindow(MessageBoxResult.OK);
        }

        private void ButYes_OnClick(object sender, RoutedEventArgs e)
        {
            CloseWindow(MessageBoxResult.Yes);
        }

        private void ButNo_OnClick(object sender, RoutedEventArgs e)
        {
            CloseWindow(MessageBoxResult.No);
        }

        private void ButCancel_OnClick(object sender, RoutedEventArgs e)
        {
            CloseWindow(MessageBoxResult.Cancel);
        }

        private void CloseWindow(MessageBoxResult result)
        {
            CustomMessageBox.Tag = result;
            CustomMessageBox.Close();
        }
    }
}