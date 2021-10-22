using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace MultiRPC.UI
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow() : this(new MainPage()) { }
        
        private readonly Control _control;
        public MainWindow(Control control)
        {
            _control = control;
            InitializeComponent();
            InitializeExtra();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeExtra()
        {
            var lang = new Language("MultiRPC");
            if (_control is ITitlePage titlePage)
            {
                lang.TextObservable.Subscribe(s =>
                {
                    txtTitle.Text = s + " - " + titlePage.Title.Text;
                });
                titlePage.Title.TextObservable.Subscribe(s =>
                {
                    txtTitle.Text = lang.Text + " - " + s;
                });
            }
            else
            {
                txtTitle.DataContext = lang;
            }
            
            eabTitleBar.PointerPressed += (sender, args) =>
            {
                BeginMoveDrag(args);
            };

            Opened += async (sender, args) =>
            {
                //TODO: See why we need this
                await Task.Delay(10);
                eabTitleBar.Height = tbrTitleBar.DesiredSize.Height;
                icon.Height = eabTitleBar.Height - icon.Margin.Top - icon.Margin.Bottom;
                icon.Width = icon.Height;
                _control.Margin += new Thickness(0, eabTitleBar.Height, 0, 0);
                grdContent.Children.Insert(1, _control);
            };
        }
    }

    public static class MainWindowExt
    {
        public static bool TryClose(this UserControl userControl)
        {
            if (userControl.Parent?.Parent is MainWindow window)
            {
                window.Close();
                return true;
            }

            return false;
        }
    }
}
