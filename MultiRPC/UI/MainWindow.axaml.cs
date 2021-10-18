using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MultiRPC.UI
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow() : this(new MainPage()) 
        { }
        
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
            txtTitle.DataContext = new Language("MultiRPC");
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
                eabBackground.Margin = new Thickness(0, eabTitleBar.Height, 0, 0);
                _control.Margin += eabBackground.Margin;
                grdContent.Children.Insert(1, _control);
            };
        }
    }
}
