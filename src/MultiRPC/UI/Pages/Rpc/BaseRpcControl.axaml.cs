using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Discord;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Controls;

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
public partial class BaseRpcControl : Grid, ITabPage
{
    private bool _lastIDCheckStatus = true;
    private readonly DisableSettings _disableSettings = SettingManager<DisableSettings>.Setting;
    private bool _lastValid;

    public Presence RichPresence { get; set; } = null!;
    public ImagesType ImageType { get; set; }
    public bool GrabID { get; init; }
    public Language? TabName { get; init; }
    public bool IsDefaultPage => true;

    public bool RpcValid => 
        stpContent.Children
            .Skip(1) //txtClientID is always the first control, just skip it
            .Where(x => x.DataContext is ControlValidation)
            .Select(x => ((ControlValidation?)x.DataContext)?.LastResultStatus)
            .All(x => x.GetValueOrDefault(true)) && _lastIDCheckStatus;

    public event EventHandler<bool>? PresenceValidChanged;
    public event EventHandler? ProfileChanged;

    public void ChangeRichPresence(Presence richPresence)
    {
        RichPresence = richPresence;
        if (!IsInitialized)
        {
            return;
        }
            
        txtClientID.Text = richPresence.Id.ToString();
        txtText1.Text = richPresence.Profile.Details;
        txtText2.Text = richPresence.Profile.State;
        txtLargeText.Text = richPresence.Profile.LargeText;
        txtSmallText.Text = richPresence.Profile.SmallText;
        txtButton1Url.Text = richPresence.Profile.Button1Url;
        txtButton1Text.Text = richPresence.Profile.Button1Text;
        txtButton2Url.Text = richPresence.Profile.Button2Url;
        txtButton2Text.Text = richPresence.Profile.Button2Text;
        ckbElapsedTime.IsChecked = richPresence.Profile.ShowTime;
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
                //TODO: Add token check 
            }
        };
        
        if (GrabID)
        {
            txtClientID.IsVisible = true;
            txtClientID.AddValidation(LanguageText.ClientID, null, s =>
            {
                if (s.Length is > 19 or < 18)
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
            }, OnProfileChanged, RichPresence.Id.ToString());
        }

        txtText1.AddValidation(LanguageText.Text1, s => RichPresence.Profile.Details = s, s => s.Check(), OnProfileChanged, RichPresence.Profile.Details);
        txtText2.AddValidation(LanguageText.Text2, s => RichPresence.Profile.State = s, s => s.Check(), OnProfileChanged, RichPresence.Profile.State);
        txtLargeText.AddValidation(LanguageText.LargeText, s => RichPresence.Profile.LargeText = s, s => s.Check(), OnProfileChanged, RichPresence.Profile.LargeText);
        txtSmallText.AddValidation(LanguageText.SmallText, s => RichPresence.Profile.SmallText = s, s => s.Check(), OnProfileChanged, RichPresence.Profile.SmallText);

        txtButton1Url.AddValidation(LanguageText.Button1Url, s => RichPresence.Profile.Button1Url = s, x => x.CheckUrl(), OnProfileChanged, RichPresence.Profile.Button1Url);
        txtButton1Text.AddValidation(LanguageText.Button1Text, s => RichPresence.Profile.Button1Text = s, s => s.Check(32), OnProfileChanged, RichPresence.Profile.Button1Text);
        txtButton2Url.AddValidation(LanguageText.Button2Url, s => RichPresence.Profile.Button2Url = s, x => x.CheckUrl(), OnProfileChanged, RichPresence.Profile.Button2Url);
        txtButton2Text.AddValidation(LanguageText.Button2Text, s => RichPresence.Profile.Button2Text = s, s => s.Check(32), OnProfileChanged, RichPresence.Profile.Button2Text);

        ckbElapsedTime.IsChecked = RichPresence.Profile.ShowTime;
        ckbElapsedTime.DataContext = (Language)LanguageText.ShowElapsedTime;
    }
    
    public void AddExtraControl(Control control)
    {
        Children.Add(control);
    }

    public void SetLargeControl(Control control)
    {
        stpContent.Children.Insert(stpContent.Children.IndexOf(txtLargeText), control);
    }
    
    public void SetSmallControl(Control control)
    {
        stpContent.Children.Insert(stpContent.Children.IndexOf(txtSmallText), control);
    }
    
    public void OnProfileChanged(bool _)
    {
        ProfileChanged?.Invoke(this, EventArgs.Empty);
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

    private void CkbElapsedTime_OnChange(object? sender, RoutedEventArgs e)
    {
        RichPresence.Profile.ShowTime = ckbElapsedTime.IsChecked.GetValueOrDefault(false);
    }
}