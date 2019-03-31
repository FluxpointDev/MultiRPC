using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MultiRPC.GUI
{
    //Want to make a custom MessageBox soon™
    class CustomMessageBox : MainWindow
    {
        public CustomMessageBox(MessageBoxPage page, string title)
        {
            InitializeComponent();
            SizeToContent = SizeToContent.WidthAndHeight;
            ContentFrame.Content = page;
            MinHeight = page.MinHeight + 30;
            MinWidth = page.MinWidth;
            tblTitle.Text = title;
            butMin.Visibility = Visibility.Collapsed;
            ShowInTaskbar = false;
        }

        private static async Task<MessageBoxResult> _Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, MessageBoxResult messageBoxResult = MessageBoxResult.None)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                long tick = DateTime.Now.Ticks;
                MessageBoxPage page = new MessageBoxPage(messageBoxText, messageBoxButton, messageBoxImage, tick);
                var window = new CustomMessageBox(page, messageBoxTitle);
                window.WindowID = tick;
                if (messageBoxButton == MessageBoxButton.YesNo)
                    window.butClose.IsEnabled = false;
                window.ShowDialog();
                return window.ToReturn ?? DefaultReturn(messageBoxButton, messageBoxResult);
            });
            return messageBoxResult;
        }

        private static MessageBoxResult DefaultReturn(MessageBoxButton button, MessageBoxResult defaultMessageBoxResult)
        {
            if (button == MessageBoxButton.OKCancel)
            {
                return MessageBoxResult.Cancel;
            }
            if (button == MessageBoxButton.OK)
            {
                return MessageBoxResult.OK;
            }
            if (button == MessageBoxButton.YesNoCancel)
            {
                return MessageBoxResult.Cancel;
            }
            return defaultMessageBoxResult;
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText)
        {
            return await _Show(messageBoxText, "MultiRPC", MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle)
        {
            return await _Show(messageBoxText, messageBoxTitle, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton)
        {
            return await _Show(messageBoxText, messageBoxTitle, messageBoxButton, MessageBoxImage.None);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            return await _Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, MessageBoxResult defaultResult)
        {
            return await _Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage);
        }
    }
}
