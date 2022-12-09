using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Svg;
using Avalonia.Threading;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Helpers;
using MultiRPC.Rpc;
using PropertyChanged.SourceGenerator;
using Splat;

namespace MultiRPC.UI.Views;

public enum ViewType
{
    NotSet,
    Default,
    Default2,
    Loading,
    Error,
    LocalRichPresence,
    RpcRichPresence
}

[Flags]
enum UpdateType
{
    All = 1,
    Text = 2,
    Tooltips = 4,
    SmallImage = 8,
    LargeImage = 16,
}

public partial class RpcViewText
{
    [Notify] private string? _title;
    [Notify] private string? _text1;
    [Notify] private string? _text2;
}

//TODO: Readd Error
//TODO: Update view for small image if it's the only image selected
public partial class RpcView : Border
{
    [Notify] private ViewType _viewType = ViewType.NotSet;
    [Notify] private Presence? _rpcProfile;
    
    private readonly RpcViewText _viewText = new();
    private readonly Run _runTitle = new()
    {
        [!Run.TextProperty] = new Binding("Title"), 
        FontWeight = FontWeight.SemiBold
    };
    private readonly Run _runText1 = new(){ [!Run.TextProperty] = new Binding("Text1") };
    private readonly Run _runText2 = new() { [!Run.TextProperty] = new Binding("Text2") };

    private SvgImage _logoImage;
    private SvgImage _errorImage;
    private Stream _loadingGifStream; //TODO: Update on new asset

    //TODO: Make it so we show this (Readding error)
    private string? _rpcError;
    private static readonly RpcClient RPCClient = Locator.Current.GetService<RpcClient>() ?? throw new NoRpcClientException();
    
    private DateTime? _timerTime;
    private readonly DispatcherTimer _timer;

    private static readonly SemaphoreSlim ImageLock = new SemaphoreSlim(1, 1);
    private static readonly Dictionary<Uri, Stream> CachedStreams = new Dictionary<Uri, Stream>();
    
    public RpcView()
    {
        InitializeComponent();

        //We want to do this so we update the text when the language changes
        LanguageGrab.LanguageChanged += (sender, args) => OnViewTypeChanged();

        //Get needed assets
        _logoImage = SvgImageHelper.LoadImage("Logo.svg");
        _errorImage = SvgImageHelper.LoadImage("Icons/Delete.svg");
        _loadingGifStream = AssetManager.GetSeekableStream("Loading.gif");
        AssetManager.RegisterForAssetReload("Logo.svg", () => UpdateAssetImage(ref _logoImage, "Logo.svg"));
        AssetManager.RegisterForAssetReload("Icons/Delete.svg", () => UpdateAssetImage(ref _errorImage, "Icons/Delete.svg"));

        //Do bindings
        this.GetPropertyChangedObservable(BackgroundProperty).Subscribe(x =>
        {
            if (!x.IsEffectiveValueChange)
            {
                return;
            }
            
            //TODO: Check some other brushes
            //If we have the purple brush then we want the text to be white, else use whatever ThemeForegroundBrush is
            if (x.GetNewValue<IBrush?>()?.Equals(Application.Current?.Resources["PurpleBrush"]) ?? false)
            {
                tblContent.Foreground = Brushes.White;
            }
            else
            {
                tblContent[!TextBlock.ForegroundProperty] = new Binding("ThemeForegroundBrush");
            }
        });
        
        _runTitle.DataContext = _runText1.DataContext = _runText2.DataContext = _viewText;
        //BUG: I should be able to do this in XAML but it seems to not work correctly when I do...
        sliLargeImage[!IsVisibleProperty] = MakeImageSourceBinding(sliLargeImage);
        gridSmallImage[!IsVisibleProperty] = new MultiBinding
        {
            Converter = BoolConverters.And,
            Bindings =
            {
                sliLargeImage[!IsVisibleProperty],
                MakeImageSourceBinding(sliSmallImage),
            }
        };

        /*BUG: I should be able to use a VisualBrush but it seem to not work, this does the job as the size doesn't change*/
        sliSmallImage.OpacityMask = new ImageBrush(new Ellipse
        {
            Width = sliSmallImage.Width,
            Height = sliSmallImage.Height,
            Fill = Brushes.White
        }.RenderToBitmap());
        sliLargeImage.OpacityMask = new ImageBrush(new Border
        {
            Width = sliLargeImage.Width,
            Height = sliLargeImage.Height,
            Background = Brushes.White,
            CornerRadius = new CornerRadius(5)
        }.RenderToBitmap());

        //Set content inlines
        tblContent.Inlines = new InlineCollection
        {
            _runTitle,
            _runText1,
            _runText2,
        };
        
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, TimerTicked);
        RPCClient.Disconnected += (sender, args) => StopTimer();
        RPCClient.PresenceUpdated += (sender, message) =>
        {
            if (ViewType == ViewType.RpcRichPresence)
            {
                Dispatcher.UIThread.Post(() => UpdateProfile(message.Presence, message.Name, long.Parse(message.ApplicationID)));
            }
        };
    }

    //TODO: Move this somewhere else
    private void UpdateProfile(DiscordRPC.BaseRichPresence presence, string name, long id)
    {
        _timerTime = presence.Timestamps?.Start;
        if (_timerTime != null)
        {
            _timer.Start();
        }
        else
        {
            StopTimer();
        }
        
        //TODO: See if we can change this, really don't like that we need to do this....
        if (RpcProfile == null || RpcProfile.Id != id)
        {
            RpcProfile = new Presence(name, id)
            {
                Profile = new RpcProfile
                {
                    State = presence.State,
                    Details = presence.Details,
                    LargeKey = presence.Assets?.LargeImageKey,
                    LargeText = presence.Assets?.LargeImageText,
                    SmallKey = presence.Assets?.SmallImageKey,
                    SmallText = presence.Assets?.SmallImageText,
                    ShowTime = presence.Timestamps?.Start != null,
                }
            };
            return;
        }
        
        var profile = RpcProfile.Profile;
        profile.State = presence.State;
        profile.Details = presence.Details;
        profile.LargeKey = presence.Assets?.LargeImageKey;
        profile.LargeText = presence.Assets?.LargeImageText;
        profile.SmallKey = presence.Assets?.SmallImageKey;
        profile.SmallText = presence.Assets?.SmallImageText;
        profile.ShowTime = presence.Timestamps?.Start != null;
    }

    //Rehook into the presence so we can correctly grab changes
    private async void OnRpcProfileChanged(Presence? oldValue, Presence? newValue)
    {
        if (oldValue != null)
        {
            oldValue.Profile.PropertyChanged -= OnProfilePropertyChanged;
        }

        if (newValue == null)
        {
            _viewText.Title = _viewText.Text1 = _viewText.Text2 = null;
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(async () => await UpdateFromPresence(newValue));
        newValue.Profile.PropertyChanged += OnProfilePropertyChanged;
    }

    private async void OnViewTypeChanged()
    {
        this[!BackgroundProperty] = GetBackgroundBinding();
        switch (ViewType)
        {
            case ViewType.NotSet:
            {
                UpdateText();
                UpdateImage(sliLargeImage);
                UpdateImage(sliSmallImage);
            } break;
            case ViewType.Default:
            {
                UpdateText(LanguageText.MultiRPC, LanguageText.ThankYouForUsing, LanguageText.ThisProgram);
                UpdateImage(sliLargeImage, _logoImage);
                UpdateImage(sliSmallImage);
            } break;
            case ViewType.Default2:
            {
                UpdateText(LanguageText.MultiRPC, LanguageText.Hello, LanguageText.World);
                UpdateImage(sliLargeImage, _logoImage);
                UpdateImage(sliSmallImage);
            } break;
            case ViewType.Loading:
            {
                UpdateText(LanguageText.Loading);
                UpdateImage(sliLargeImage, stream: _loadingGifStream);
                UpdateImage(sliSmallImage);
            } break;
            case ViewType.Error:
            {
                UpdateText(Language.GetText(LanguageText.Error), _rpcError);
                UpdateImage(sliLargeImage, _errorImage);
                UpdateImage(sliSmallImage);
            } break;
            case ViewType.LocalRichPresence or ViewType.RpcRichPresence:
            {
                await UpdateFromPresence(RpcProfile);
            } break;
        }
    }

    private async Task UpdateFromPresence(Presence? presence, UpdateType updateType = UpdateType.All)
    {
        if (presence == null || ViewType is not (ViewType.LocalRichPresence or ViewType.RpcRichPresence))
        {
            return;
        }

        var profile = presence.Profile;
        if (updateType.HasAnyFlag(UpdateType.Text | UpdateType.All))
        {
            UpdateText(RpcProfile?.Name ?? Language.GetText(LanguageText.UnknownProfile), profile.Details, profile.State);
        }

        if (updateType.HasAnyFlag(UpdateType.Tooltips | UpdateType.All))
        {
            sliLargeImage.UpdateTooltip(profile.LargeText);
            sliSmallImage.UpdateTooltip(profile.SmallText);
        }
        
        if (updateType.HasAnyFlag(UpdateType.LargeImage | UpdateType.All))
        {
            await UpdateLargeImage(presence.AssetsManager.GetUri(profile.LargeKey));
        }

        if (updateType.HasAnyFlag(UpdateType.SmallImage | UpdateType.All))
        {
            await UpdateSmallImage(presence.AssetsManager.GetUri(profile.SmallKey));
        }
    }
    
    private void StopTimer()
    {
        _timer.Stop();
        tblTime.Text = string.Empty;
    }

    private Task UpdateSmallImage(Uri? uri) => UpdateImageFromUri(uri, sliSmallImage);
    private Task UpdateLargeImage(Uri? uri) => UpdateImageFromUri(uri, sliLargeImage);

    private async Task UpdateImageFromUri(Uri? uri, SwitchableImage switchableImage)
    {
        if (uri == null)
        {
            switchableImage.ClearView();
            return;
        }

        //We need to reset the stream here or only one frame will be shown
        using (await ImageLock.UseWaitAsync())
        {
            _loadingGifStream.Seek(0, SeekOrigin.Begin);
            switchableImage.SourceStream = _loadingGifStream;
        }

        if (await CacheImageFromUri(uri))
        {
            var memStream = new MemoryStream();

            using (await ImageLock.UseWaitAsync())
            {
                var stream = CachedStreams[uri];
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
            }

            switchableImage.SourceStream = memStream;
            return;
        }
        
        switchableImage.ClearView();
    }

    private static async Task<bool> CacheImageFromUri(Uri uri)
    {
        using (await ImageLock.UseWaitAsync())
        {
            if (CachedStreams.ContainsKey(uri))
            {
                return true;
            }
        }

        var imageResponse = await App.HttpClient.GetResponseMessageAsync(uri);
        if (imageResponse is not { IsSuccessStatusCode: true })
        {
            return false;
        }

        var imageStream = await imageResponse.Content.ReadAsStreamAsync();
        using (await ImageLock.UseWaitAsync())
        {
            CachedStreams[uri] = imageStream;
        }
        return true;
    }
    
    private void UpdateText(LanguageText title, LanguageText? text1 = null, LanguageText? text2 = null) => 
        UpdateText(Language.GetText(title), 
        text1 == null ? null : Language.GetText(text1), 
        text2 == null ? null : Language.GetText(text2));

    private void UpdateText(string? title = null, string? text1 = null, string? text2 = null)
    {
        _viewText.Title = !string.IsNullOrWhiteSpace(title) ? title + Environment.NewLine + Environment.NewLine : null;
        _viewText.Text1 = !string.IsNullOrWhiteSpace(text1) ? text1 + Environment.NewLine : null;
        _viewText.Text2 = text2;
    }

    //TODO: Maybe do binding so we don't need this?
    /// <summary>
    /// Updates the <see cref="SvgImage"/> and reapplies it to the images
    /// </summary>
    /// <param name="svgImage"><see cref="SvgImage"/> to update</param>
    /// <param name="path">Path of the <see cref="SvgImage"/></param>
    private void UpdateAssetImage(ref SvgImage? svgImage, string path)
    {
        var updateLargeImage = sliLargeImage.SourceImage?.Equals(svgImage) ?? false;
        var updateSmallImage = sliSmallImage.SourceImage?.Equals(svgImage) ?? false;
        svgImage = SvgImageHelper.LoadImage(path);

        if (updateLargeImage)
        {
            sliLargeImage.SourceImage = svgImage;
        }
        if (updateSmallImage)
        {
            sliSmallImage.SourceImage = svgImage;
        }
    }
    
    /// <summary>
    /// Updates the image to the new source (or nothing if no source is provided)
    /// </summary>
    /// <param name="control">Control to update</param>
    /// <param name="image"><see cref="IImage"/> to show in the control</param>
    /// <param name="stream">Image <see cref="Stream"/> content to show in the control</param>
    private static async void UpdateImage(SwitchableImage control, IImage? image = null, Stream? stream = null)
    {
        if (image != null && stream != null)
        {
            throw new Exception("Only image or stream can be set, not both!");
        }
        
        control.ClearView();
        if (image is not null)
        {
            control.SourceImage = image;
        }
        if (stream is not null)
        {
            using (await ImageLock.UseWaitAsync())
            {
                stream.Seek(0, SeekOrigin.Begin);
                control.SourceStream = stream;
            }
        }
        control.UpdateTooltip(null);
    }

    private async void OnProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Rpc.RpcProfile.Details) or nameof(Rpc.RpcProfile.State):
                await UpdateFromPresence(RpcProfile, UpdateType.Text);
                return;
            case nameof(Rpc.RpcProfile.LargeText) or nameof(Rpc.RpcProfile.SmallText):
                await UpdateFromPresence(RpcProfile, UpdateType.Tooltips);
                return;
            case nameof(Rpc.RpcProfile.LargeKey):
                await UpdateFromPresence(RpcProfile, UpdateType.LargeImage);
                return;
            case nameof(Rpc.RpcProfile.SmallKey):
                await UpdateFromPresence(RpcProfile, UpdateType.SmallImage);
                return;
        }
    }

    private void TimerTicked(object? sender, EventArgs e)
    {
        if (!_timerTime.HasValue)
        {
            tblTime.Text = null;
            return;
        }
        var ts = DateTime.UtcNow.Subtract(_timerTime.Value);

        var text = ts.Hours > 0 ? ts.Hours.ToString("00") + ":" : null;
        text += $"{ts.Minutes:00}:{ts.Seconds:00}";
        tblTime.Text = text;
    }

    private static MultiBinding MakeImageSourceBinding(SwitchableImage image) =>
        new()
        {
            Converter = BoolConverters.Or,
            Bindings =
            {
                new Binding
                {
                    Path = "SourceImage",
                    Source = image,
                    Converter = ObjectConverters.IsNotNull
                },
                new Binding
                {
                    Path = "SourceStream",
                    Source = image,
                    Converter = ObjectConverters.IsNotNull
                }
            }
        };

    private IBinding GetBackgroundBinding()
    {
        var resources = Application.Current!.Resources;
        return ViewType switch
        {
            ViewType.Error => resources.GetResourceObservable("RedBrush").ToBinding(),
            ViewType.LocalRichPresence or ViewType.RpcRichPresence => resources.GetResourceObservable("PurpleBrush").ToBinding(),
            _ => resources.GetResourceObservable("ThemeAccentBrush2").ToBinding(),
        };
    }
}