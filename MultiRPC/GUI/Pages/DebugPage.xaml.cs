using System;
using System.Windows;
using MultiRPC.GUI.Views;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        }

        private void DebugPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
        }

        private Task UpdateText()
        {
            btnDebugStartRPC.Content = App.Text.DebugRPCStart;
            btnDebugStopRPC.Content = App.Text.DebugRPCStop;
            btnDebugStartRPC.ToolTip = new ToolTip(App.Text.DebugStartRPCTooltip);
            btnDebugStopRPC.ToolTip = new ToolTip(App.Text.DebugStopRPCTooltip);
            btnTestUpdate.Content = App.Text.TestUpdateWindow;
            btnTestUpdate.ToolTip = new ToolTip(App.Text.TestUpdateWindowTooltip);
            return Task.CompletedTask;
        }

        private void ButDebugStartRPC_OnClick(object sender, RoutedEventArgs e)
        {
            ((RPCPreview)MainPage._MainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Loading);
            RPC.SetPresence(App.Text.Testing, App.Text.DebugMode, "debug", App.Text.BeepBoop, "", "", false);
            RPC.IDToUse = 450894077165043722;
            RPC.Start();
        }

        private void ButDebugStopRPC_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.Shutdown();
            ((RPCPreview)MainPage._MainPage.frameRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Default);
        }

        private async void ButTestUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            var tick = DateTime.Now.Ticks;
            var page = new UpdatePage(null, tick);
            await MainWindow.OpenWindow(page, true, tick, false);
        }
    }
}
