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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiRPC.GUI.Views;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for DebugPage.xaml
    /// </summary>
    public partial class DebugPage : Page
    {
        public DebugPage()
        {
            InitializeComponent();
            Loaded += DebugPage_Loaded;
            UpdateText();
        }

        private void DebugPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
        }

        public async Task UpdateText()
        {
            btnDebugStartRPC.Content = App.Text.DebugRPCStart;
            btnDebugStopRPC.Content = App.Text.DebugRPCStop;
            btnDebugStartRPC.ToolTip = new ToolTip(App.Text.DebugStartRPCTooltip);
            btnDebugStopRPC.ToolTip = new ToolTip(App.Text.DebugStopRPCTooltip);
            btnDebugSteam.Content = App.Text.RPCSteam;
            btnDebugSteam.ToolTip = new ToolTip(App.Text.RPCSteamTooltip);
            btnTestUpdate.Content = App.Text.TestUpdateWindow;
            btnTestUpdate.ToolTip = new ToolTip(App.Text.TestUpdateWindowTooltip);

        }

        private void ButDebugStartRPC_OnClick(object sender, RoutedEventArgs e)
        {
            ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Loading);
            RPC.SetPresence(App.Text.Testing, App.Text.DebugMode, "debug", App.Text.BeepBoop, "", "", false);
            RPC.IDToUse = 450894077165043722;
            RPC.Start();
        }

        private void ButDebugStopRPC_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.Shutdown();
            ((RPCPreview)MainPage.mainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Default);
        }

        private void ButDebugSteam_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.IDToUse = 450894077165043722;
            RPC.Start(tbDebugSteamID.Text);
        }

        private void ButTestUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                var tick = DateTime.Now.Ticks;
                var page = new UpdatePage(null, tick);
                var window = new MainWindow(page, false);
                window.WindowID = tick;
                window.ShowDialog();
            });
        }
    }
}
