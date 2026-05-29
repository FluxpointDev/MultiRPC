using System.Net.NetworkInformation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MultiRPC.Extensions;
using MultiRPC.Utils;

namespace MultiRPC.UI.Overlays;

public partial class NetworkStatusOverlay : Panel
{
    public NetworkStatusOverlay()
    {
        InitializeComponent();
        NetworkChange.NetworkAddressChanged += AddressChangedCallback;

        tblInternetConnectivity.DataContext = _textLang = new Language();
        AddressChangedCallback(null, EventArgs.Empty);
    }

    private readonly Language _textLang;
    private void AddressChangedCallback(object? sender, EventArgs e)
    {
        if (NetworkUtil.NetworkIsAvailable())
        {
            this.RunUILogic(async () =>
            {
                this.Background = (SolidColorBrush)Application.Current.Resources["GreenBrush"]!;
                _textLang.ChangeJsonNames(LanguageText.InternetBack);
                await Task.Delay(3000);
                this.Height = 0;
            });
            return;
        }
            
        this.RunUILogic(() =>
        {
            this.Height = double.NaN;
            _textLang.ChangeJsonNames(LanguageText.InternetLost);
            this.Background = (SolidColorBrush)Application.Current.Resources["RedBrush"]!;
        });
    }
}