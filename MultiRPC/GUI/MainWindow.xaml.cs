using MultiRPC.Core.Notification;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Page pageToShow)
        {
            InitializeComponent();
            Title += Core.Utils.RunningAsAdministrator ? "- Administrator" : "";
            tblTitle.Text = Title;
            Name = "win" + DateTime.Now.Ticks;
            RegisterName(Name, this);

            if (pageToShow == null)
            {
                NotificationCenter.Logger.Warning($"{nameof(pageToShow)} is null!! window {Name} will have no content if not added before being shown");
            }

            //Give this the glass frame, allows the window to be more native
            Style style = null;
            if (SystemParameters.IsGlassEnabled)
            {
                style = (Style)Resources["GlassStyle"];
            }

            Style ??= style;
            //Go into "fallback" mode, where we do all content like before
            if (Style == null)
            {
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                recHandle.MouseLeftButtonDown += (sender, args) => DragMove();
                brdContent.BorderThickness = new Thickness(1);
                Closing += MainWindow_Closing;
                Activated += (_, __) => _ = brdContent.ThicknessAnimation(new Thickness(1), brdContent.BorderThickness, propertyDependency: Border.BorderThicknessProperty);
                Deactivated += (_, __) => _ = brdContent.ThicknessAnimation(new Thickness(0), brdContent.BorderThickness, propertyDependency: Border.BorderThicknessProperty);
            }
            else
            {
                //Give the titlebar the padding of the glass frame to allow it to not overlap with the glass frame
                var frameThickness = WindowChrome.GetWindowChrome(this).GlassFrameThickness;
                gridtitleBar.Margin = new Thickness(frameThickness.Left, frameThickness.Top, frameThickness.Right, 0);
            }

            frmContent.Content = pageToShow;
        }

        //This is the fallback animation for closing when the user doesn't/can't have glass enabled
        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            ShowInTaskbar = false;

            await this.DoubleAnimation(0, propertyDependency: OpacityProperty, duration: new Duration(TimeSpan.FromSeconds(0.5)));
            Closing -= MainWindow_Closing;
            Close();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMin_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
