using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Svg;
using MultiRPC.Extensions;
using MultiRPC.Helpers;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Pages.Rpc.Popups;

namespace MultiRPC.UI.Pages.Rpc;

//TODO: Make a toolbar which can also have a popup when they is too little space to show all profiles
public partial class CustomPage : Border, IRpcPage
{
    public string IconLocation => "Icons/Custom";
    public string LocalizableName => "Custom";
    public Presence RichPresence
    {
        get => _activeProfile;
        protected set
        {
            //Don't do anything
        }
    }
    public bool PresenceValid => _rpcControl.RpcValid;
    public Thickness ContentPadding { get; } = new Thickness(0);
    public event EventHandler? PresenceChanged;
    public event EventHandler<bool>? PresenceValidChanged;

    private readonly ProfilesSettings _profilesSettings = SettingManager<ProfilesSettings>.Setting;
    private readonly DisableSettings _disableSettings = SettingManager<DisableSettings>.Setting;
    private readonly Dictionary<string, IBitmap> _helpImages = new Dictionary<string, IBitmap>();
    private Image _helpImage = null!;
    private Image? _selectedHelpImage;
    private Presence _activeProfile = null!;
    private IDisposable? _textBindingDis;
    private Button? _activeButton;
    private BaseRpcControl _rpcControl = null!;
    private SvgImage _svgHelpImage = null!;
    private readonly Language _editLang = LanguageText.ProfileEdit;
    private readonly Language _shareLang = LanguageText.ProfileShare;
    private readonly Language _addLang = LanguageText.ProfileAdd;
    private readonly Language _deleteLang = LanguageText.ProfileDelete;
    private readonly Language _cloneLang = LanguageText.ProfileClone;
    private readonly AutoCompleteBox txtLargeKey = new AutoCompleteBox
    {
        [!AutoCompleteBox.TextProperty] = new Binding("Result", BindingMode.TwoWay),
        [!AutoCompleteBox.WatermarkProperty] = new Binding("Lang.TextObservable^")
    };
    private readonly AutoCompleteBox txtSmallKey = new AutoCompleteBox
    {
        [!AutoCompleteBox.TextProperty] = new Binding("Result", BindingMode.TwoWay),
        [!AutoCompleteBox.WatermarkProperty] = new Binding("Lang.TextObservable^")
    };

    public void Initialize(bool loadXaml)
    {
        InitializeComponent(loadXaml);

        //All of this page logic goes here
        _svgHelpImage = SvgImageHelper.LoadImage("Icons/Help.svg");
        AssetManager.RegisterForAssetReload("Icons/Help.svg", () => _svgHelpImage = SvgImageHelper.LoadImage("Icons/Help.svg"));
        btnProfileEdit.AddSvgAsset("Icons/Pencil.svg");
        btnProfileShare.AddSvgAsset("Icons/Share.svg");
        btnProfileAdd.AddSvgAsset("Icons/Add.svg");
        btnProfileDelete.AddSvgAsset("Icons/Delete.svg");
        btnProfileClone.AddSvgAsset("Icons/Clone.svg");

        //Setup tooltips
        _editLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnProfileEdit, x));
        _shareLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnProfileShare, x));
        _addLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnProfileAdd, x));
        _deleteLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnProfileDelete, x));
        _cloneLang.TextObservable.Subscribe(x => CustomToolTip.SetTip(btnProfileClone, x));
            
        //Make help controls
        var helpGrid = new Grid
        {
            Margin = new Thickness(5, 0, 0, 0),
            ColumnDefinitions = new ColumnDefinitions("Auto *")
        };
        Grid.SetColumn(helpGrid, 1);
            
        //TODO: Replace image with new images and make help for buttons
        var stpHelpIcons = new StackPanel { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Left };
        string[] helpImages = { "ClientID.jpg", "Text1.jpg", "Text2.jpg", "SmallAndLargeKey.jpg", "LargeText.jpg", "SmallAndLargeKey.jpg", "SmallText.jpg" };
        stpHelpIcons.Children.AddRange(helpImages.Select(MakeHelpImage));
        stpHelpIcons.IsVisible = !_disableSettings.HelpIcons;
        helpGrid.Children.Add(stpHelpIcons);

        _helpImage = new Image
        {
            Height = 200, 
            Width = double.NaN,
            Margin = new Thickness(10,0,0,0), 
            Opacity = 0,
            IsVisible = stpHelpIcons.IsVisible,
        };
        Grid.SetColumn(_helpImage, 1);
        helpGrid.Children.Add(_helpImage);
            
        _disableSettings.PropertyChanged += (sender, args) =>
        {
            stpHelpIcons.IsVisible = !_disableSettings.HelpIcons;
            _helpImage.IsVisible = stpHelpIcons.IsVisible;
        };
        
        //All of the Rpc control goes here
        _rpcControl = new BaseRpcControl
        {
            ImageType = ImagesType.Custom,
            GrabID = true,
            TabName = LanguageText.CustomPage,
            Margin = new Thickness(10),
        };
        _rpcControl.ProfileChanged += (sender, args) => PresenceChanged?.Invoke(sender, args);
        _rpcControl.PresenceValidChanged += (sender, b) => PresenceValidChanged?.Invoke(sender, b);

        //Process current profiles and setup for processing new profiles and profiles that get deleted
        wrpProfileSelector.Children.AddRange(_profilesSettings.Profiles.Select(MakeProfileSelector));
        _profilesSettings.Profiles.CollectionChanged += (sender, args) =>
        {
            foreach (Presence profile in args.OldItems ?? Array.Empty<object>())
            {
                wrpProfileSelector.Children.Remove(wrpProfileSelector.Children.First<IControl>(x => x.DataContext == profile));
            }
            foreach (Presence profile in args.NewItems ?? Array.Empty<object>())
            {
                wrpProfileSelector.Children.Add(MakeProfileSelector(profile));
            }
        };
        BtnChangePresence(wrpProfileSelector.Children[_profilesSettings.LastSelectedProfileIndex], null!);

        //TODO: Add datacontext to autocompletebox
        //Process the tab pages
        var tabPage = new TabsPage();
        Grid.SetRow(tabPage, 2);
        grdContent.Children.Insert(grdContent.Children.Count - 1, tabPage);
        tabPage.AddTab(_rpcControl);
        tabPage.Initialize();
        _rpcControl.AddExtraControl(helpGrid);
        _rpcControl.SetLargeControl(txtLargeKey);
        _rpcControl.SetSmallControl(txtSmallKey);
        _ = GetAssets();
    }

    private Image MakeHelpImage(string helpImage)
    {
        var image = new Image { Classes = { "help" }, Tag = helpImage, Source = _svgHelpImage };
        AssetManager.ReloadAssets += (sender, args) => image.Source = _svgHelpImage;
        image.PointerPressed += ImageOnPointerPressed;
        return image;
    }

    private void ImageOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var image = (Image)sender!;

        if (_selectedHelpImage != null)
        {
            _selectedHelpImage.Classes.Remove("selected");
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (_selectedHelpImage == image)
            {
                _selectedHelpImage = null;
                _helpImage.Opacity = 0;
                return;
            }
        }

        _selectedHelpImage = image;
        _selectedHelpImage.Classes.Add("selected");
        _helpImage.Opacity = 1;

        var key = image.Tag!.ToString()!;
        if (!_helpImages.ContainsKey(key))
        {
            var path = "Images/Help/" + key;
            AssetManager.RegisterForAssetReload(path, () =>
            {
                _helpImages[key] = new Bitmap(AssetManager.GetSeekableStream(path));
                //If this is the selected image then reshow it
                if (image.Tag!.ToString()! == _selectedHelpImage.Tag!.ToString()!)
                {
                    _helpImage.Source = _helpImages[key];
                }
            });
            _helpImages[key] = new Bitmap(AssetManager.GetSeekableStream(path));
        }

        _helpImage.Source = _helpImages[key];
    }
    
    private void AddTextBinding()
    {
        var textBinding = new Binding
        {
            Source = _activeProfile, 
            Mode = BindingMode.OneWay,
            Path = nameof(_activeProfile.Name)
        };
        _textBindingDis = tblProfileName.Bind(TextBlock.TextProperty, textBinding);
    }

    private Control MakeProfileSelector(Presence presence)
    {
        var btn = new Button
        {
            DataContext = presence,
            Margin = new Thickness(0, 0, 5, 0)
        };
        btn.Click += BtnChangePresence;
        var binding = new Binding
        {
            Source = presence,
            Path = nameof(presence.Name)
        };
        btn.Bind(Button.ContentProperty, binding);
        return btn;
    }

    private void BtnChangePresence(object? sender, RoutedEventArgs e)
    {
        _activeButton?.Classes.Remove("activeProfile");
        _activeButton = (Button)sender!;
        _activeButton.Classes.Add("activeProfile");
        _profilesSettings.LastSelectedProfileIndex = wrpProfileSelector.Children.IndexOf(_activeButton);
            
        _activeProfile = (Presence)_activeButton.DataContext!;
        _textBindingDis?.Dispose();
        AddTextBinding();
        btnProfileDelete.IsVisible = !_profilesSettings.Profiles.First().Equals(_activeProfile);

        /* This sets the controls for rpc page */
        txtLargeKey.Text = _activeProfile.Profile.LargeKey;
        txtSmallKey.Text = _activeProfile.Profile.SmallKey;

        _rpcControl.ChangeRichPresence(_activeProfile);
        RichPresence = _activeProfile;
    }

    private async void ImgProfileEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new MainWindow(new EditPage(_activeProfile)) { DisableMinimiseButton = true };
        await window.ShowDialog();
    }

    private async void ImgProfileShare_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new MainWindow(new SharePage(_activeProfile)) { DisableMinimiseButton = true };
        await window.ShowDialog();
    }

    private void ImgProfileAdd_OnClick(object? sender, RoutedEventArgs e)
    {
        var newProfile = new Presence("Profile" + _profilesSettings.Profiles.Count, 0);
        _profilesSettings.Profiles.Add(newProfile);
        BtnChangePresence(wrpProfileSelector.Children[^1], e);
    }

    private void ImgProfileDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        var profileIndex = wrpProfileSelector.Children.IndexOf(_activeButton);
        //Makes sure we "Click" on a valid button
        if (profileIndex == _profilesSettings.Profiles.Count - 1)
        {
            profileIndex--;
        }
        _profilesSettings.Profiles.Remove(_activeProfile);
        BtnChangePresence(wrpProfileSelector.Children[profileIndex], e);
    }

    private void ImgProfileClone_OnClick(object? sender, RoutedEventArgs e)
    {
        var presenceContext = RichPresenceContext.Default.Presence;
        var profile = JsonSerializer.Deserialize(JsonSerializer.Serialize(_activeProfile, presenceContext), presenceContext);
        
        var profiles = SettingManager<ProfilesSettings>.Setting.Profiles;
        profiles.CheckName(profile);
        profiles.Add(profile);

        BtnChangePresence(wrpProfileSelector.Children[^1], e);
    }
    
    /*From here this is all of the RPC logic we add into the BaseRpcControl*/
    
    private async Task GetAssets()
    {
        //TODO: See why our assets crash avalonia ui when selecting item in autocomplete
        _rpcControl.ProfileChanged += async (sender, args) =>
        {
            await RichPresence.AssetsManager.GetAssetsAsync();
            //txtLargeKey.Items = RichPresence.AssetsManager.Assets?.Select(x => x.Name).ToImmutableArray();
            //txtSmallKey.Items = txtLargeKey.Items;
        };

        //TODO: Add a way to check if it's an url and to check the url
        txtLargeKey.AddValidation(LanguageText.LargeKey, s => RichPresence.Profile.LargeKey = s, s => s.Check(256), OnProfileChanged, RichPresence.Profile.LargeKey);
        txtSmallKey.AddValidation(LanguageText.SmallKey, s => RichPresence.Profile.SmallKey = s, s => s.Check(256), OnProfileChanged, RichPresence.Profile.SmallKey);
            
        await RichPresence.AssetsManager.GetAssetsAsync();
        //txtLargeKey.Items = RichPresence.AssetsManager.Assets?.Select(x => x.Name).ToImmutableArray();
        //txtSmallKey.Items = txtLargeKey.Items;
    }
    
    private void OnProfileChanged(bool e)
    {
        _rpcControl.OnProfileChanged(e);
    }
}