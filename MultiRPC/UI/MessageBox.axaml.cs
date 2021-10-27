using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MultiRPC.UI
{
    public enum MessageBoxResult
    {
        None,
        Cancel,
        OK,
        Yes,
        No
    }

    public enum MessageBoxButton
    {
        OKCancel,
        OK,
        YesNo,
        YesNoCancel
    }
    
    public enum MessageBoxImage
    {
        None,
    }
    
    public partial class MessageBox : UserControl
    {
        public MessageBox()
        {
            if (!Design.IsDesignMode)
            {
                throw new Exception("Don't call this const");
            }
        }

        public MessageBox(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            InitializeComponent();
            
            btnNo.Content = Language.GetText("No");
            btnYes.Content = Language.GetText("Yes");
            btnOk.Content = Language.GetText("Ok");
            btnCancel.Content = Language.GetText("Cancel");
            tblText.Text = messageBoxText;

            if (messageBoxButton == MessageBoxButton.OK)
            {
                btnOk.IsVisible = true;
            }
            if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                btnOk.IsVisible = true;
                btnCancel.IsVisible = true;
            }
            if (messageBoxButton == MessageBoxButton.YesNo)
            {
                btnYes.IsVisible = true;
                btnNo.IsVisible = true;
            }
            if (messageBoxButton == MessageBoxButton.YesNoCancel)
            {
                btnYes.IsVisible = true;
                btnNo.IsVisible = true;
                btnCancel.IsVisible = true;
            }

            //TODO: Add
            var imgInt = (int)messageBoxImage;
            if (imgInt == 0)
            {
                imgStatus.IsVisible = false;
            }
            else if (imgInt == 64)
            {
                //imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "InfoIconDrawingImage");
            }
            else if (imgInt == 48)
            {
                //imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "WarningIconDrawingImage");
            }
            else if (imgInt == 16)
            {
                //imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "AlertIconDrawingImage");
            }
            else if (imgInt == 32)
            {
                //imgMessageBoxImage.SetResourceReference(Image.SourceProperty, "HelpIconDrawingImage");
            }
        }

        private void ButOk_OnClick(object? sender, RoutedEventArgs e)
        {
            this.TryClose(MessageBoxResult.OK);
        }

        private void ButYes_OnClick(object? sender, RoutedEventArgs e)
        {
            this.TryClose(MessageBoxResult.Yes);
        }

        private void ButNo_OnClick(object? sender, RoutedEventArgs e)
        {
            this.TryClose(MessageBoxResult.No);
        }

        private void ButCancel_OnClick(object? sender, RoutedEventArgs e)
        {
            this.TryClose(MessageBoxResult.Cancel);
        }
        
        private static MessageBoxResult DefaultReturn(MessageBoxButton button, MessageBoxResult defaultMessageBoxResult)
        {
            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    return MessageBoxResult.Cancel;
                case MessageBoxButton.OK:
                    return MessageBoxResult.OK;
                case MessageBoxButton.YesNoCancel:
                    return MessageBoxResult.Cancel;
                case MessageBoxButton.YesNo:
                    return MessageBoxResult.Yes;
                default:
                    return defaultMessageBoxResult;
            }
        }
        
        private static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage,
            MessageBoxResult messageBoxResult = MessageBoxResult.None, Window? ownerWindow = null)
        {
            var page = new MessageBox(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage);
            switch (messageBoxButton)
            {
                case MessageBoxButton.OKCancel:
                    page.btnOk.IsDefault = true;
                    break;
                case MessageBoxButton.OK:
                    page.btnOk.IsDefault = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    page.btnYes.IsDefault = true;
                    break;
                case MessageBoxButton.YesNo:
                    page.btnYes.IsDefault = true;
                    break;
            }

            var window = new MainWindow(page)
            {
                SizeToContent = SizeToContent.WidthAndHeight,
                MinHeight = page.MinHeight + 30,
                MinWidth = page.MinWidth,
                Title = messageBoxTitle,
                CanResize = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };

            ownerWindow ??= ((App)Application.Current).DesktopLifetime?.MainWindow;
            return await window.ShowDialog<MessageBoxResult?>(ownerWindow) ?? DefaultReturn(messageBoxButton, messageBoxResult);
        }
        
        public static async Task<MessageBoxResult> Show(string messageBoxText, Window? window = null)
        {
            return await Show(messageBoxText, "MultiRPC", MessageBoxButton.OK, MessageBoxImage.None,
                ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            Window? window = null)
        {
            return await Show(messageBoxText, messageBoxTitle, MessageBoxButton.OK, MessageBoxImage.None,
                ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, Window? window = null)
        {
            return await Show(messageBoxText, messageBoxTitle, messageBoxButton, MessageBoxImage.None,
                ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, Window? window = null)
        {
            return await Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage, ownerWindow: window);
        }
    }
}