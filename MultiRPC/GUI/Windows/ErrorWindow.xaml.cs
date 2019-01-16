using MultiRPC.Data;
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
                Error.Text = "This is a nice error window dont you agree :D";
                Height += 10;
            }
            else
            {
                if (App.WD != null)
                {
                    try
                    {
                        _Data.SaveProfiles();
                        App.Config.Save();
                    }
                    catch { }
                }
                string Username = Environment.UserName;
                foreach (string l in ErrorMessage.Split('\\'))
                {
                    if (l.Contains(Username))
                    {
                        Username = l;
                        break;
                    }
                }
                Error.Text = ErrorMessage.Replace("C:\\Users\\Brandan Lees\\documents\\visual studio 2017\\Projects\\MultiRPC\\MultiRPC\\", "\\").Replace("Brandan Lees", "User");
            }
        }

        public void SetError(DispatcherUnhandledExceptionEventArgs error)
        {
            Exception ex = error.Exception;
            ErrorMessage = $"{ex.Message}\n\n{ex.ToString().Replace("C:\\Users\\Brandan Lees\\documents\\visual studio 2017\\Projects\\MultiRPC\\MultiRPC\\", "\\").Replace("Brandan Lees", "User")}";
        }

        public void SetUpdateError(Exception ex)
        {
            Title = "Update Error";
            ErrorMessage = $"{ex.Message}\n\n{ex.ToString().Replace("C:\\Users\\Brandan Lees\\documents\\visual studio 2017\\Projects\\MultiRPC\\MultiRPC\\", "\\").Replace("Brandan Lees", "User")}";
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

        private void Dev_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(App.Developer);
        }
    }
}
