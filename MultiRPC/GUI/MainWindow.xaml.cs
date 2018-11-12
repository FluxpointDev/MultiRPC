using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WD = this;
            LiveRPC.Content = new ViewRPCControl(ViewType.Default);
            //Back_Window.Margin = new Thickness(0, 0, 10, 10);
        }
        public static MainWindow WD;
        public static void SetLiveView(DiscordRPC.Message.PresenceMessage msg)
        {
            WD.LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.LiveRPC.Content = new ViewRPCControl(msg);
            });
        }

        public static void SetLiveView(ViewType view, string error = "")
        {
            WD.LiveRPC.Dispatcher.BeginInvoke((Action)delegate ()
            {
                WD.LiveRPC.Content = new ViewRPCControl(view, error);
            });
        }
    }
}
