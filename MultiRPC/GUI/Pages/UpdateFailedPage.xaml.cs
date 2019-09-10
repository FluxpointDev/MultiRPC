using System;
using System.Windows;
using System.Windows.Controls;
using MultiRPC.Functions;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for UpdateFailedPage.xaml
    /// </summary>
    public partial class UpdateFailedPage : Page
    {
        private readonly long _windowID;

        public UpdateFailedPage(Exception e, long windowID)
        {
            InitializeComponent();
            _windowID = windowID;
            tblUpdateFailed.Text = App.Text.UpdateFailed;
            tblUpdateFailedMessage.Text = App.Text.UpdateFailedMessage;
            btnRetryUpdate.Content = App.Text.RetryUpdate;
            btnRetryOnNextStartUp.Content = App.Text.RetryOnNextStartUp;
            tbUpdateError.Text = e.Message;
            Title = App.Text.UpdateFailed;
        }

        private void ButRetryUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(_windowID);
            Updater.Update();
        }

        private void ButRetryOnNextStartUp_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CloseWindow(_windowID);
        }
    }
}