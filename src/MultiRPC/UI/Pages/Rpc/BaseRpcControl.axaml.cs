using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using MultiRPC.Discord;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Controls;
using TinyUpdate.Core.Extensions;

namespace MultiRPC.UI.Pages.Rpc;

public enum ImagesType
{
    /// <summary>
    /// Use images from the user's RPC and not our images
    /// </summary>
    Custom,
    /// <summary>
    /// Use the images from us
    /// </summary>
    BuiltIn
}

//TODO: reset id border if setting changed to not check for id
public partial class BaseRpcControl : UserControl, ITabPage
{
    private static string[]? _localizedMultiRPCAssetsNames;
    private static readonly ProfileAssetsManager MultiRPCAssetManager = ProfileAssetsManager.GetOrAddManager(Constants.MultiRPCID);

    private bool _lastIDCheckStatus = true;
    private readonly DisableSettings _disableSettings = SettingManager<DisableSettings>.Setting;
    private bool _lastValid;

    public RichPresence RichPresence { get; set; } = null!;
    public ImagesType ImageType { get; set; }
    public bool GrabID { get; init; }
    public Language? TabName { get; init; }
    public bool IsDefaultPage => true;

    public bool RpcValid => 
        stpContent.Children
            .Where(x => x is TextBox && x.Name != nameof(txtClientID))
            .Select(x => ((ControlValidation?)((TextBox)x).DataContext)?.LastResultStatus)
            .All(x => x.GetValueOrDefault(true)) && _lastIDCheckStatus;

    public event EventHandler<bool>? PresenceValidChanged;
    public event EventHandler? ProfileChanged;

    public void ChangeRichPresence(RichPresence richPresence)
    {
        RichPresence = richPresence;

        if (!IsInitialized)
        {
            return;
        }
            
        txtClientID.Text = richPresence.Id.ToString();
        txtText1.Text = richPresence.Profile.Details;
        txtText2.Text = richPresence.Profile.State;
        txtLargeKey.Text = richPresence.Profile.LargeKey;
        txtLargeText.Text = richPresence.Profile.LargeText;
        txtSmallKey.Text = richPresence.Profile.SmallKey;
        txtSmallText.Text = richPresence.Profile.SmallText;
        txtButton1Url.Text = richPresence.Profile.Button1Url;
        txtButton1Text.Text = richPresence.Profile.Button1Text;
        txtButton2Url.Text = richPresence.Profile.Button2Url;
        txtButton2Text.Text = richPresence.Profile.Button2Text;
        ckbElapsedTime.IsChecked = richPresence.Profile.ShowTime;
    }
        
    private void OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        txtClientID.Text = RichPresence.Id.ToString();
        this.AttachedToLogicalTree -= OnAttachedToLogicalTree;
    }

    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        ProfileChanged += (sender, args) =>
        {
            var isValid = RpcValid;
            if (_lastValid != isValid)
            {
                PresenceValidChanged?.Invoke(this, isValid);
            }
            _lastValid = isValid;
        };
        _disableSettings.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DisableSettings.TokenCheck))
            {
                    
            }
        };
        
        if (GrabID)
        {
            txtClientID.IsVisible = true;
            txtClientID.AddValidation(Language.GetLanguage(LanguageText.ClientID), null, s =>
            {
                if (s.Length != 18)
                {
                    _lastIDCheckStatus = false;
                    ProfileChanged?.Invoke(this, EventArgs.Empty);
                    return new CheckResult(false, Language.GetText(LanguageText.ClientIDMustBe18CharactersLong));
                }

                if (!_disableSettings.TokenCheck)
                {
                    _ = CheckID(s);
                }
                else
                {
                    _lastIDCheckStatus = true;
                }
                return new CheckResult(true);
            }, OnProfileChanged);
            this.AttachedToLogicalTree += OnAttachedToLogicalTree;
        }

        txtText1.AddValidation(Language.GetLanguage(LanguageText.Text1), s => RichPresence.Profile.Details = s, Check, OnProfileChanged, RichPresence.Profile.Details);
        txtText2.AddValidation(Language.GetLanguage(LanguageText.Text2), s => RichPresence.Profile.State = s, Check, OnProfileChanged, RichPresence.Profile.State);
        txtLargeText.AddValidation(Language.GetLanguage(LanguageText.LargeText), s => RichPresence.Profile.LargeText = s, Check, OnProfileChanged, RichPresence.Profile.LargeText);
        txtSmallText.AddValidation(Language.GetLanguage(LanguageText.SmallText), s => RichPresence.Profile.SmallText = s, Check, OnProfileChanged, RichPresence.Profile.SmallText);

        txtButton1Url.AddValidation(Language.GetLanguage(LanguageText.Button1Url), s => RichPresence.Profile.Button1Url = s, x => CheckUrl(x), OnProfileChanged, RichPresence.Profile.Button1Url);
        txtButton1Text.AddValidation(Language.GetLanguage(LanguageText.Button1Text), s => RichPresence.Profile.Button1Text = s, s => Check(s, 32), OnProfileChanged, RichPresence.Profile.Button1Text);
        txtButton2Url.AddValidation(Language.GetLanguage(LanguageText.Button2Url), s => RichPresence.Profile.Button2Url = s, x => CheckUrl(x), OnProfileChanged, RichPresence.Profile.Button2Url);
        txtButton2Text.AddValidation(Language.GetLanguage(LanguageText.Button2Text), s => RichPresence.Profile.Button2Text = s, s => Check(s, 32), OnProfileChanged, RichPresence.Profile.Button2Text);

        ckbElapsedTime.IsChecked = RichPresence.UseTimestamp;
        ckbElapsedTime.DataContext = Language.GetLanguage(LanguageText.ShowElapsedTime);

        _ = GetAssets();
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
    
    private async Task GetAssets()
    {
        //TODO: See why our assets crash avalonia ui when selecting item in autocomplete
        if (ImageType == ImagesType.Custom)
        {
            cboLargeKey.IsVisible = false;
            cboSmallKey.IsVisible = false;

            txtLargeKey.IsVisible = true;
            txtSmallKey.IsVisible = true;

            ProfileChanged += async (sender, args) =>
            {
                await RichPresence.AssetsManager.GetAssetsAsync();
                //txtLargeKey.Items = RichPresence.AssetsManager.Assets?.Select(x => x.Name).ToImmutableArray();
                //txtSmallKey.Items = txtLargeKey.Items;
            };

            //TODO: Add a way to check if it's an url and to check the url
            txtLargeKey.AddValidation(Language.GetLanguage(LanguageText.LargeKey), s => RichPresence.Profile.LargeKey = s, s => Check(s, 256), OnProfileChanged, RichPresence.Profile.LargeKey);
            txtSmallKey.AddValidation(Language.GetLanguage(LanguageText.SmallKey), s => RichPresence.Profile.SmallKey = s, s => Check(s, 256), OnProfileChanged, RichPresence.Profile.SmallKey);
            
            await RichPresence.AssetsManager.GetAssetsAsync();
            //txtLargeKey.Items = RichPresence.AssetsManager.Assets?.Select(x => x.Name).ToImmutableArray();
            //txtSmallKey.Items = txtLargeKey.Items;
            return;
        }
        
        await MultiRPCAssetManager.GetAssetsAsync();
        if (MultiRPCAssetManager.Assets == null)
        {
            //TODO: Do something else like retry later
            return;
        }
        
        Language.LanguageChanged += (sender, args) =>
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
        var largeKey = MultiRPCAssetManager.Assets.IndexOf(x => x?.Name == RichPresence.Profile.LargeKey);
        if (largeKey == -1)
        {
            largeKey = 0;
        }
        cboLargeKey.SelectedIndex = largeKey;

        var smallKey = MultiRPCAssetManager.Assets.IndexOf(x => x?.Name == RichPresence.Profile.SmallKey);
        if (smallKey == -1)
        {
            smallKey = 0;
        }
        cboSmallKey.SelectedIndex = smallKey;
    }
        
    private void OnProfileChanged(bool _)
    {
        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddExtraControl(Control control)
    {
        gidContent.Children.Add(control);
    }

    private async Task CheckID(string s)
    {
        txtClientID.Classes.Remove("error");
        _lastIDCheckStatus = false;

        string? error = null;
        if (long.TryParse(s, out var id))
        {
            txtClientID.Classes.Add("checking");
            var (successful, resultMessage) = await IDChecker.Check(id);
            _lastIDCheckStatus = successful;
                
            txtClientID.Classes.Remove("checking");
            if (successful)
            {
                CustomToolTip.SetTip(txtClientID, null);
                RichPresence.Id = id;
                if (RichPresence.Name.StartsWith("Profile"))
                {
                    RichPresence.Name = resultMessage!;
                }
                ProfileChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            error = resultMessage;
        }
        txtClientID.Classes.Add("error");
        error ??= Language.GetText(LanguageText.ClientIDIsNotValid);
        CustomToolTip.SetTip(txtClientID, error);
        ProfileChanged?.Invoke(this, EventArgs.Empty);
    }
        
    private CheckResult CheckUrl(string s, int byteCount = 512)
    {
        if (string.IsNullOrWhiteSpace(s) || Uri.TryCreate(s, UriKind.Absolute, out _))
        {
            return s.CheckBytes(byteCount)
                ? new CheckResult(true)
                : new CheckResult(false, Language.GetText(LanguageText.UriTooBig));
        }

        return new CheckResult(false, Language.GetText(LanguageText.InvalidUri));
    }

    private static CheckResult Check(string s) => Check(s, 128);
    private static CheckResult Check(string s, int max)
    {
        if (s.Length == 1)
        {
            return new CheckResult(false, Language.GetText(LanguageText.OneChar));
        }

        return s.CheckBytes(max)
            ? new CheckResult(true)
            : new CheckResult(false, Language.GetText(LanguageText.TooManyChars));
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

    private void CkbElapsedTime_OnChange(object? sender, RoutedEventArgs e)
    {
        RichPresence.UseTimestamp = ckbElapsedTime.IsChecked.GetValueOrDefault(false);
    }
}