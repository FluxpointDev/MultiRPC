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
        public static async Task<MessageBoxResult> Show(string messageBoxText)
        {
            return MessageBox.Show(messageBoxText, "MultiRPC");
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle)
        {
            return MessageBox.Show(messageBoxText, messageBoxTitle);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton)
        {
            return MessageBox.Show(messageBoxText, messageBoxTitle, messageBoxButton);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            return MessageBox.Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage);
        }

        public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, MessageBoxResult defaultResult)
        {
            return MessageBox.Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage, defaultResult);
        }
    }
}
