using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MultiRPC.Extensions;
using MultiRPC.Utils;

namespace MultiRPC.UI.Overlays
{
    public partial class NetworkStatusOverlay : UserControl
    {
        public NetworkStatusOverlay()
        {
            InitializeComponent();
            NetworkChange.NetworkAddressChanged += AddressChangedCallback;
            AddressChangedCallback(null, EventArgs.Empty);
        }

        private void AddressChangedCallback(object? sender, EventArgs e)
        {
            if (NetworkUtil.NetworkIsAvailable())
            {
                this.RunUILogic(async () =>
                {
                    this.Background = (SolidColorBrush)Application.Current.Resources["GreenBrush"]!;
                    tblInternetConnectivity.Text = Language.GetText("InternetBack") + "!!";
                    await Task.Delay(3000);
                    this.Height = 0;
                });
                return;
            }
            
            this.RunUILogic(() =>
            {
                this.Height = double.NaN;
                tblInternetConnectivity.Text = Language.GetText("InternetLost") + "!!";
                this.Background = (SolidColorBrush)Application.Current.Resources["RedBrush"]!;
            });
        }
    }
}