using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window 
    {
        public bool Test = false;
        string ErrorMessage = "";
        public ErrorWindow()
        {
            InitializeComponent();
            Loaded += ErrorWindow_Loaded;
        }

        private void ErrorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LabelDev.Content = App.Developer;
            if (Test)
            {
                Title = "TEST - Error";
                Error.Text = "Grrr what are you looking at";
                Height += 10;
            }
            else
            {
                try
                {
                    RPC.Config.Save(App.WD);
                }
                catch { }
                string Username = Environment.UserName;
                foreach (string l in ErrorMessage.Split('\\'))
                {
                    if (l.Contains(Username))
                    {
                        Username = l;
                        break;
                    }
                }
                Error.Text = ErrorMessage.Replace(Username, "(USER)");
            }
        }

        public void SetError(DispatcherUnhandledExceptionEventArgs error)
        {
            Exception ex = error.Exception;
            ErrorMessage = $"{ex.Message}\n\n{ex.ToString()}";
        }

        public void SetUpdateError(Exception ex)
        {
            Title = "Update Error";
            ErrorMessage = $"{ex.Message}\n\n{ex.ToString()}";
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (Test)
                Close();
            else
                Application.Current.Shutdown();
        }

        private void BtnDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(App.SupportServer);
        }
    }
}
