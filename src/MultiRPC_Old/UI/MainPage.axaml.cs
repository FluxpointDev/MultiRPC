using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Svg;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Pages;
using ShimSkiaSharp;

namespace MultiRPC.UI;

//TODO: Cleanup
public partial class MainPage : Grid
{
    private Button? _selectedBtn;
    private readonly IRpcPage? _autoStartPage;
    private readonly DisableSettings _disableSetting = SettingManager<DisableSettings>.Setting;
    private string? _lastColourName;
    private Color? _lastColor;
    private IDisposable? _currentColourBinding;
    public MainPage()
    {
        InitializeComponent();
        _disableSetting.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DisableSettings.AcrylicEffect))
            {
                if (!string.IsNullOrWhiteSpace(_lastColourName))
                {
                    UpdateFromResource();
                    return;
                }
            
                UpdateFromColor();
            }
        };

        //Make all the buttons which will put onto the sidebar,
        //finding the autostart page if we have it
        Button? btnToTrigger = null;
        ISidePage? pageToTrigger = null;
        var autoStartPageName = SettingManager<GeneralSettings>.Setting.AutoStart;
        foreach (var page in PageManager.CurrentPages)
        {
            var btn = AddSidePage(page);
            btnToTrigger ??= btn;
            pageToTrigger ??= page;
            if (page.LocalizableName == autoStartPageName)
            {
                btnToTrigger = btn;
                pageToTrigger = page;
            }
        }
        PageManager.PageAdded += (sender, page) => AddSidePage(page);
        SideButton_Clicked(btnToTrigger, null!, pageToTrigger!);

        //If auto start has been selected then we want load that up if possible
        if (pageToTrigger?.LocalizableName == autoStartPageName)
        {
            _autoStartPage = (IRpcPage)pageToTrigger;
            if (_autoStartPage.PresenceValid)
            {
                TriggerStart();
                return;
            }

            //If the presence isn't yet valid then wait for a
            //bit and see if it becomes valid
            _ = WaitForValidPresence();
        }
    }

    private async Task WaitForValidPresence()
    {
        _autoStartPage!.PresenceValidChanged += OnPresenceValidChanged;
        await Task.Delay(5000);
        _autoStartPage.PresenceValidChanged -= OnPresenceValidChanged;
    }
        
    private void OnPresenceValidChanged(object? sender, bool e)
    {
        if (e)
        {
            TriggerStart();
            _autoStartPage!.PresenceValidChanged -= OnPresenceValidChanged;
        }
    }

    private void TriggerStart()
    {
        if (ReferenceEquals(_autoStartPage, cclContent.Content))
        {
            topbar.TriggerStartStop();
        }
    }

    private Button AddSidePage(ISidePage page)
    {
        var key = page.IconLocation + ".svg";
        var source = AssetManager.LoadSvgImage(key);
        UpdateIconColour(source, (Color)Application.Current.Resources["ThemeAccentColor3"]!);
        var img = new Image
        {
            Margin = new Thickness(4.5),
            Source = new SvgImage
            {
                Source = source
            }
        };
        var btn = new Button
        {
            Content = img,
            Tag = source
        };
        AssetManager.RegisterForAssetReload(key, () =>
        {
            var newSource = AssetManager.LoadSvgImage(page.IconLocation + ".svg");
            UpdateIconColour(newSource, (Color)Application.Current.Resources["ThemeAccentColor3"]!);
            img.Source = new SvgImage
            {
                Source = newSource
            };
            btn.Tag = newSource;
        });

        //Update the colour based on if it's the current page or not when this changes
        Application.Current.GetResourceObservable("ThemeAccentColor3").Subscribe(x =>
        {
            if (!ReferenceEquals(_selectedBtn, btn))
            {
                UpdateButtonIconColour(btn, (Color)x!);
            }
        });
        Application.Current.GetResourceObservable("NavButtonSelectedIconColor").Subscribe(x =>
        {
            if (ReferenceEquals(_selectedBtn, btn))
            {
                UpdateButtonIconColour(btn, (Color)x!);
            }
        });

        Language lang = page.LocalizableName;
        lang.TextObservable.Subscribe(s => CustomToolTip.SetTip(btn,  _disableSetting.ShowPageTooltips ? null : s));
        _disableSetting.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DisableSettings.ShowPageTooltips))
            {
                CustomToolTip.SetTip(btn, _disableSetting.ShowPageTooltips ? null : lang.Text);
            }
        };

        btn.Click += (sender, args) => SideButton_Clicked(sender, args, page);
        btn.Classes.Add("nav");
        splPages.Children.Add(btn);
        return btn;
    }

    private void UpdateButtonIconColour(ContentControl btn, Color color)
    {
        var source = (SvgSource)btn.Tag!;
        UpdateIconColour(source, color);
        ((Image)btn.Content).Source = new SvgImage
        {
            Source = source
        };
    }

    private void UpdateIconColour(SvgSource source, Color color)
    {
        foreach (var commands in source.Picture?.Commands ?? ArraySegment<CanvasCommand>.Empty)
        {
            if (commands is DrawPathCanvasCommand pathCanvasCommand)
            {
                pathCanvasCommand.Paint.Shader = SKShader.CreateColor(new SKColor(color.R, color.G, color.B, color.A), SKColorSpace.Srgb);
            }
        }
    }

    private void SideButton_Clicked(object? sender, RoutedEventArgs e, ISidePage page)
    {
        if (sender is not Button btn 
            || ReferenceEquals(btn, _selectedBtn))
        {
            return;
        }

        if (_selectedBtn != null)
        {
            UpdateButtonIconColour(_selectedBtn, (Color)Application.Current.Resources["ThemeAccentColor3"]!);
            _selectedBtn.Classes.Remove("selected");
        }
        btn.Classes.Insert(0,"selected");
        UpdateButtonIconColour(btn, (Color)Application.Current.Resources["NavButtonSelectedIconColor"]!);

        _selectedBtn = btn;
        if (!page.IsInitialized)
        {
            page.Initialize();
        }
        
        //Remove old binding
        _currentColourBinding?.Dispose();

        _lastColor = page.PageBackground;
        if (!page.PageBackground.HasValue)
        {
            _lastColourName = !string.IsNullOrWhiteSpace(page.BackgroundResourceName)
                              && App.Current.Resources.ContainsKey(page.BackgroundResourceName)
                ? page.BackgroundResourceName
                : "ThemeAccentColor";
        
            // Get new colours/brushes
            var obs = App.Current.GetResourceObservable(_lastColourName);
            _currentColourBinding = obs.Subscribe(x => UpdateFromResource());
            UpdateFromResource();
        }
        else
        {
            _lastColourName = null;
            UpdateFromColor();
        }
        
        cclContent.Padding = page.ContentPadding;
        cclContent.Content = page;
        cclContent.Height = page.Height;
        if (page is IRpcPage rpcPage)
        {
            RpcPageManager.NewActivePage(rpcPage);
        }
    }

    private void UpdateFromColor() => contentBorder.Background = new ImmutableSolidColorBrush(_lastColor.Value, _disableSetting.AcrylicEffect ? 1 : 0.7);
    private void UpdateFromResource()
    {
        var resource = App.Current.Resources[_lastColourName];
        contentBorder.Background = resource is IBrush brush ? brush : new ImmutableSolidColorBrush((Color)resource, _disableSetting.AcrylicEffect ? 1 : 0.7);
    }
}