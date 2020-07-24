using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MultiRPC.GUI.Views;
using MultiRPC.JsonClasses;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    /// <summary>
    /// Interaction logic for DebugPage.xaml
    /// </summary>
    public partial class DebugPage : Page
    {
        public static DebugPage _DebugPage;
        private readonly CustomProfile DebugProfile = new CustomProfile
        {
            Text1 = App.Text.Testing,
            Text2 = App.Text.DebugMode,
            LargeKey = "debug",
            LargeText = App.Text.BeepBoop
        };
        
        public DebugPage()
        {
            InitializeComponent();
            _DebugPage = this;
            UpdateText();
        }

        public Task UpdateText()
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
            ((RPCPreview) MainPage._MainPage.frmRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Loading);

            DebugProfile.Text1 = App.Text.Testing;
            DebugProfile.Text2 = App.Text.DebugMode;
            DebugProfile.LargeText = App.Text.BeepBoop;
            RPC.SetPresence(DebugProfile);
            RPC.IDToUse = 450894077165043722;
            RPC.Start();
        }

        private void ButDebugStopRPC_OnClick(object sender, RoutedEventArgs e)
        {
            RPC.Shutdown();
            ((RPCPreview) MainPage._MainPage.frmRPCPreview.Content).UpdateUIViewType(RPCPreview.ViewType.Default);
        }

        private async void ButTestUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            var tick = DateTime.Now.Ticks;
            await MainWindow.OpenWindow(new UpdatePage(null, tick), true, tick, false);
        }
    }
}