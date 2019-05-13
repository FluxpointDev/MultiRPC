using System;
using System.Windows;
using MultiRPC.Functions;
using System.Windows.Controls;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for UpdateFailedPage.xaml
    /// </summary>
    public partial class UpdateFailedPage : Page
    {
        private long windowID;
        public UpdateFailedPage(Exception e, long _windowID)
        {
            InitializeComponent();
            windowID = _windowID;
            tblUpdateFailed.Text = App.Text.UpdateFailed;
            tblUpdateFailedMessage.Text = App.Text.UpdateFailedMessage;
            btnRetryUpdate.Content = App.Text.RetryUpdate;
            btnRetryOnNextStartUp.Content = App.Text.RetryOnNextStartUp;
            tbUpdateError.Text = e.Message;
            Title = App.Text.UpdateFailed;
        }

        private void ButRetryUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(windowID);
            Updater.Start();
        }

        private void ButRetryOnNextStartUp_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(windowID);
        }
    }
}
