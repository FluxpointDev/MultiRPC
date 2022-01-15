using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MultiRPC.Exceptions;
using MultiRPC.Rpc;
using Splat;
using System;

namespace MultiRPC.UI.Pages;

public partial class DebugPage : UserControl, ISidePage
{
    public DebugPage()
    {
        InitializeComponent();
    }
    private RpcClient _rpcClient;

    public string IconLocation => "Icons/Debug";
    public string LocalizableName => "Debug";
    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);
        _rpcClient = Locator.Current.GetService<RpcClient>() ?? throw new NoRpcClientException();

        tblRPC.DataContext = Language.GetLanguage(LanguageText.RPC);
        tblGUI.DataContext = Language.GetLanguage(LanguageText.GUI);

        btnDebugStartRPC.DataContext = Language.GetLanguage(LanguageText.DebugRPCStart);
        btnDebugStopRPC.DataContext = Language.GetLanguage(LanguageText.DebugRPCStop);
        btnTestUpdate.DataContext = Language.GetLanguage(LanguageText.TestUpdateWindow);

        var debugStartRPCTooltip = Language.GetLanguage(LanguageText.DebugStartRPCTooltip);
        var debugStopRPCTooltip = Language.GetLanguage(LanguageText.DebugStopRPCTooltip);
        var testUpdateWindowTooltip = Language.GetLanguage(LanguageText.TestUpdateWindowTooltip);
        debugStartRPCTooltip.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnDebugStartRPC, x));
        debugStopRPCTooltip.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnDebugStopRPC, x));
        testUpdateWindowTooltip.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnTestUpdate, x));
    }

    public void Initialize() => Initialize(true);

    public IBrush? BackgroundColour { get; }
    public Thickness ContentPadding { get; } = new Thickness(10);

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
        await _rpcClient.UpdatePresence(new RichPresence(debug, ID)
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