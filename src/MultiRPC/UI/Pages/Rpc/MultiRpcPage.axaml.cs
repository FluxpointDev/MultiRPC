using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MultiRPC.Discord;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Extensions;

namespace MultiRPC.UI.Pages.Rpc;

public partial class MultiRpcPage : Grid, IRpcPage
{
    private static string[]? _localizedMultiRPCAssetsNames;
    private static readonly ProfileAssetsManager MultiRPCAssetManager = ProfileAssetsManager.GetOrAddManager(Constants.MultiRPCID);
    private readonly IBrush _white = Brushes.White.ToImmutable();
    private ComboBox cboLargeKey = new ComboBox();
    private ComboBox cboSmallKey = new ComboBox();

    public string IconLocation => "Icons/Discord";
    public string LocalizableName => "MultiRPC";
    public RichPresence RichPresence { get; } = SettingManager<MultiRPCSettings>.Setting.Presence;
    public bool PresenceValid => rpcControl.RpcValid;
    public string? BackgroundResourceName => "ThemeAccentColor2";

    public event EventHandler? PresenceChanged;
    public event EventHandler<bool>? PresenceValidChanged;

    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        tblLookLike.DataContext = (Language)LanguageText.WhatItWillLookLike;
        AssetManager.ReloadAssets += (sender, args) =>
        {
            rpcView.UpdateBackground((IBrush)Application.Current.Resources["PurpleBrush"]!);
            rpcView.UpdateForeground(_white);
        };

        RichPresence.Id = Constants.MultiRPCID;
        rpcView.UpdateBackground((IBrush)Application.Current.Resources["PurpleBrush"]!);
        rpcView.UpdateForeground(_white);

        rpcControl.PresenceValidChanged += (sender, b) => PresenceValidChanged?.Invoke(sender, b);
        rpcControl.ProfileChanged += (sender, args) => PresenceChanged?.Invoke(sender, args);
        rpcControl.RichPresence = RichPresence;

        cboLargeKey.SelectionChanged += CboLargeKey_OnSelectionChanged;
        cboSmallKey.SelectionChanged += CboSmallKey_OnSelectionChanged;

        rpcControl.Initialize(loadXaml);
        rpcControl.SetSmallControl(cboSmallKey);
        rpcControl.SetLargeControl(cboLargeKey);
        _ = Task.Run(SetupAssets);
    }

    private async Task SetupAssets()
    {
        await MultiRPCAssetManager.GetAssetsAsync();

        this.RunUILogic(() =>
        {
            rpcView.RpcProfile = RichPresence;
            if (MultiRPCAssetManager.Assets == null)
            {
                //TODO: Do something else like retry later
                return;
            }
        
            LanguageGrab.LanguageChanged += (sender, args) =>
            {
                var largeKey = cboLargeKey.SelectedIndex;
                var smallKey = cboSmallKey.SelectedIndex;

                cboLargeKey.Items = _localizedMultiRPCAssetsNames = MultiRPCAssetManager.Assets
                    .Select(x => GetLocalizedOrTitleCase(x.Name))
                    .Prepend(Language.GetText(LanguageText.NoImage)).ToArray();
                cboSmallKey.Items = cboLargeKey.Items;
                cboLargeKey.SelectedIndex = largeKey;
                cboSmallKey.SelectedIndex = smallKey;
            };

            cboLargeKey.Items = _localizedMultiRPCAssetsNames = MultiRPCAssetManager.Assets
                .Select(x => GetLocalizedOrTitleCase(x.Name))
                .Prepend(Language.GetText(LanguageText.NoImage)).ToArray();
            cboSmallKey.Items = cboLargeKey.Items;
            var largeKey = MultiRPCAssetManager.Assets.IndexOf(x => x?.Name == RichPresence.Profile.LargeKey) + 1;
            cboLargeKey.SelectedIndex = largeKey;

            var smallKey = MultiRPCAssetManager.Assets.IndexOf(x => x?.Name == RichPresence.Profile.SmallKey) + 1;
            cboSmallKey.SelectedIndex = smallKey;
        });
    }
    
    private string GetLocalizedOrTitleCase(string s)
    {
        switch (s)
        {
            case "multirpc":
                return Language.GetText(LanguageText.MultiRPC);
            case "mmlol":
                return "MmLol";
            case "firefoxnightly":
                return "Firefox Nightly";
            case "christmas":
                return Language.GetText(LanguageText.Christmas);
            case "present":
                return Language.GetText(LanguageText.Present);
            case "popcorn":
                return Language.GetText(LanguageText.Popcorn);
            case "games":
                return Language.GetText(LanguageText.Games);
            default:
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
        }
    }
    
    private void CboLargeKey_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0
            || _localizedMultiRPCAssetsNames == null
            || MultiRPCAssetManager.Assets == null)
        {
            return;
        }

        var key = e.AddedItems[0]?.ToString();
        var ind = _localizedMultiRPCAssetsNames.IndexOf(x => x == key);
        if (ind <= 0)
        {
            RichPresence.Profile.LargeKey = string.Empty;
            return;
        }
        
        RichPresence.Profile.LargeKey = cboLargeKey.SelectedIndex != 0 ? 
            MultiRPCAssetManager.Assets[ind - 1].Name : string.Empty;
    }

    private void CboSmallKey_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0
            || _localizedMultiRPCAssetsNames == null
            || MultiRPCAssetManager.Assets == null)
        {
            return;
        }

        var key = e.AddedItems[0]?.ToString();
        var ind = _localizedMultiRPCAssetsNames.IndexOf(x => x == key);
        if (ind <= 0)
        {
            RichPresence.Profile.SmallKey = string.Empty;
            return;
        }
        
        RichPresence.Profile.SmallKey = cboSmallKey.SelectedIndex != 0 ? 
            MultiRPCAssetManager.Assets[ind - 1].Name : string.Empty;
    }
}