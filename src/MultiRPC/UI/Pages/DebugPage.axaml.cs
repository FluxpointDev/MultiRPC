using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Exceptions;
using MultiRPC.Rpc;
using Splat;

namespace MultiRPC.UI.Pages;

//TODO: See why using a StackPanel directly makes the UI broken
public partial class DebugPage : UserControl, ISidePage
{
    public DebugPage()
    {
        InitializeComponent();
    }
    private RpcClient _rpcClient;

    public string IconLocation => "Icons/Debug";
    public string LocalizableName => "Debug";
    public string? BackgroundResourceName => "ThemeAccentColor2";
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);
        _rpcClient = Locator.Current.GetService<RpcClient>() ?? throw new NoRpcClientException();

        tblRPC.DataContext = (Language)LanguageText.RPC;
        tblGUI.DataContext = (Language)LanguageText.GUI;

        btnDebugStartRPC.DataContext = (Language)LanguageText.DebugRPCStart;
        btnDebugStopRPC.DataContext = (Language)LanguageText.DebugRPCStop;
        btnTestUpdate.DataContext = (Language)LanguageText.TestUpdateWindow;

        Language debugStartRPCTooltip = LanguageText.DebugStartRPCTooltip;
        Language debugStopRPCTooltip = LanguageText.DebugStopRPCTooltip;
        Language testUpdateWindowTooltip = LanguageText.TestUpdateWindowTooltip;
        debugStartRPCTooltip.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnDebugStartRPC, x));
        debugStopRPCTooltip.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnDebugStopRPC, x));
        testUpdateWindowTooltip.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnTestUpdate, x));
    }

    private async void BtnTestUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        await MessageBox.Show("TO DO");
    }

    private void BtnDebugStopRPC_OnClick(object? sender, RoutedEventArgs e)
    {
        _rpcClient.Stop();
    }

    private const long ID = Constants.MultiRPCID;
    private async void BtnDebugStartRPC_OnClick(object? sender, RoutedEventArgs e)
    {
        var debug = Language.GetText(LanguageText.Debug);
        _rpcClient.Start(ID, debug);
        await _rpcClient.UpdatePresence(new Presence(debug, ID)
        {
            Profile = 
            {
                Details = Language.GetText(LanguageText.Testing),
                State = Language.GetText(LanguageText.DebugMode),
                LargeKey = "debug",
                SmallText = Language.GetText(LanguageText.BeepBoop)
            }
        });
    }
}