using System.Threading.Tasks;
using System.Windows;

namespace MultiRPC.GUI.MessageBox
{
    public class CustomMessageBox : MainWindow
    {
        private CustomMessageBox(string title) : base(null)
        {
            InitializeComponent();
            tblTitle.Text = title;
            btnMin.Visibility = Visibility.Collapsed;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
        }

        private static async Task<MessageBoxResult> _Show(string messageBoxText, string messageBoxTitle,
            MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage,
            MessageBoxResult messageBoxResult = MessageBoxResult.None)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var window = new CustomMessageBox(messageBoxTitle)
                {
                    Owner = App.Current.MainWindow
                };
                if (messageBoxButton == MessageBoxButton.YesNo)
                {
                    window.btnClose.IsEnabled = false;
                }

                var page = new MessageBoxPage(messageBoxText, messageBoxButton, messageBoxImage, window);

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

                window.frmContent.Navigate(page);
                window.ShowDialog();
                return window.Tag ?? DefaultReturn(messageBoxButton, messageBoxResult);
            });
            return messageBoxResult;
        }

        private static MessageBoxResult DefaultReturn(MessageBoxButton button, MessageBoxResult defaultMessageBoxResult) =>
        button switch
        {
            MessageBoxButton.OKCancel => MessageBoxResult.Cancel,
            MessageBoxButton.OK => MessageBoxResult.OK,
            MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
            MessageBoxButton.YesNo => MessageBoxResult.Yes,
            _ => defaultMessageBoxResult
        };

        /// <inheritdoc cref="System.Windows.MessageBox.Show(Window, string)"/>
        public static async Task<MessageBoxResult> Show(string messageBoxText)
        {
            return await _Show(messageBoxText, "MultiRPC", MessageBoxButton.OK, MessageBoxImage.None);
        }

        /// <inheritdoc cref="System.Windows.MessageBox.Show(Window, string, string)"/>
        public static async Task<MessageBoxResult> Show(string messageBoxText, string caption)
        { 
            return await _Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        /// <inheritdoc cref="System.Windows.MessageBox.Show(Window, string, string, MessageBoxButton)"/>
        public static async Task<MessageBoxResult> Show(string messageBoxText, string caption,
            MessageBoxButton button)
        {
            return await _Show(messageBoxText, caption, button, MessageBoxImage.None);
        }

        /// <inheritdoc cref="System.Windows.MessageBox.Show(Window, string, string, MessageBoxButton, MessageBoxImage)"/>
        public static async Task<MessageBoxResult> Show(string messageBoxText, string caption,
            MessageBoxButton button, MessageBoxImage icon)
        {
            return await _Show(messageBoxText, caption, button, icon);
        }

        /// <inheritdoc cref="System.Windows.MessageBox.Show(Window, string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult)"/>
        public static async Task<MessageBoxResult> Show(string messageBoxText, string caption,
            MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await _Show(messageBoxText, caption, button, icon);
        }
    }
}