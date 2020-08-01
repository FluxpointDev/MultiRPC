using System;
using System.Threading.Tasks;
using System.Windows;

namespace MultiRPC.GUI
{
    internal class CustomMessageBox : MainWindow
    {
        private CustomMessageBox(MessageBoxPage page, string title)
        {
            InitializeComponent();
            SizeToContent = SizeToContent.WidthAndHeight;
            frmContent.Content = page;
            MinHeight = page.MinHeight + 30;
            MinWidth = page.MinWidth;
            tblTitle.Text = title;
            btnMin.Visibility = Visibility.Collapsed;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
        }

        private static async Task<MessageBoxResult> _Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage,
            MessageBoxResult messageBoxResult = MessageBoxResult.None, Window ownerWindow = null)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var tick = DateTime.Now.Ticks;
                var page = new MessageBoxPage(messageBoxText, messageBoxButton, messageBoxImage, tick);

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

                var window = new CustomMessageBox(page, messageBoxTitle)
                {
                    WindowID = tick,
                    Owner = ownerWindow ?? App.Current.MainWindow
                };
                if (messageBoxButton == MessageBoxButton.YesNo)
                {
                    window.btnClose.IsEnabled = false;
                }

                window.ShowDialog();
                return window.ToReturn ?? DefaultReturn(messageBoxButton, messageBoxResult);
            });
            return messageBoxResult;
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

        public static async Task<MessageBoxResult> Show(string messageBoxText, Window window = null)
        {
            return await _Show(messageBoxText, "MultiRPC", MessageBoxButton.OK, MessageBoxImage.None,
                ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            Window window = null)
        {
            return await _Show(messageBoxText, messageBoxTitle, MessageBoxButton.OK, MessageBoxImage.None,
                ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, Window window = null)
        {
            return await _Show(messageBoxText, messageBoxTitle, messageBoxButton, MessageBoxImage.None,
                ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, Window window = null)
        {
            return await _Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage, ownerWindow: window);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, MessageBoxResult defaultResult,
            Window window = null)
        {
            return await _Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage, defaultResult,
                window);
        }
    }
}