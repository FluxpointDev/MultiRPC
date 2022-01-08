using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using AvaloniaGif.Decoding;
using DiscordRPC.Message;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Helpers;
using MultiRPC.Rpc;
using Splat;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.UI.Views;

public enum ViewType
{
    Default,
    Default2,
    Loading,
    Error,
    LocalRichPresence,
    RpcRichPresence
}
    
public partial class RpcView : UserControl
{
    private readonly RpcClient _rpcClient;
    static RpcView()
    {
        AssetManager.RegisterForAssetReload("Logo.svg", () => _logoVisualBrush = new VisualBrush(new Image { Source = SvgImageHelper.LoadImage("Logo.svg") }));
        AssetManager.RegisterForAssetReload("Icons/Delete.svg", () => _errorVisualBrush = new VisualBrush(new Image { Source = SvgImageHelper.LoadImage("Icons/Delete.svg") }));
        _logoVisualBrush = new VisualBrush(new Image { Source = SvgImageHelper.LoadImage("Logo.svg") });
        _errorVisualBrush = new VisualBrush(new Image { Source = SvgImageHelper.LoadImage("Icons/Delete.svg") });
    }

    public RpcView()
    {
        InitializeComponent();
        AssetManager.ReloadAssets += (sender, args) => this.RunUILogic(() => UpdateFromType());
        _rpcClient = Locator.Current.GetService<RpcClient>() ?? throw new NoRpcClientException();
        AssetManager.RegisterForAssetReload("Loading.gif",() =>
        {
            if (ViewType == ViewType.Loading)
            {
                gifLarge.SourceStream = AssetManager.GetSeekableStream("Loading.gif");
            }
        });
        gifSmallImage.SourceStream = Stream.Null;
        gifLarge.SourceStream = Stream.Null;

        brdLarge.Background = _logoVisualBrush;

        tblTitle.DataContext = _titleText;
        tblText1.DataContext = _tblText1;
        tblText2.DataContext = _tblText2;
        ViewType = ViewType.Default;
        _timer = new Timer(1000);
        _timer.Elapsed += TimerOnElapsed;
        _rpcClient.Disconnected += (sender, args) =>
        {
            _timer.Stop();
            _timerTime = null;
            tblTime.Text = string.Empty;
        };
    }
        
    private static readonly Dictionary<Uri, IBrush> CachedImages = new Dictionary<Uri, IBrush>();
    private static readonly Dictionary<Uri, Stream> CachedStreams = new Dictionary<Uri, Stream>();
    private readonly Language _titleText = new Language();
    private readonly Language _tblText1 = new Language();
    private readonly Language _tblText2 = new Language();
    private static VisualBrush _logoVisualBrush;
    private static VisualBrush _errorVisualBrush;

    private ViewType _viewType;
    public ViewType ViewType
    {
        get => _viewType;
        set
        {
            _viewType = value;
            this.RunUILogic(() => UpdateFromType());
        }
    }

    private RichPresence? _rpcProfile;
    public RichPresence? RpcProfile
    {
        get => _rpcProfile;
        set => UpdateFromRichPresence(value);
    }

    private void ProfileOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RpcProfile.Profile.LargeText))
        {
            var text = RpcProfile!.Profile.LargeText;
            text = string.IsNullOrWhiteSpace(text) ? null : text;
            CustomToolTip.SetTip(brdLarge, text);
            CustomToolTip.SetTip(gifLarge, text);
            return;
        }
            
        if (e.PropertyName == nameof(RpcProfile.Profile.SmallText))
        {
            var text = RpcProfile!.Profile.SmallText;
            text = string.IsNullOrWhiteSpace(text) ? null : text;
            CustomToolTip.SetTip(gridSmallImage, text);
            CustomToolTip.SetTip(gifSmallImage, text);
        }
    }
        
    private async void PresenceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_rpcProfile == null)
        {
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(RichPresence.CustomSmallImageUrl):
                await UpdateSmallImage(_rpcProfile.CustomSmallImageUrl);
                return;
            case nameof(RichPresence.CustomLargeImageUrl):
                await UpdateLargeImage(_rpcProfile.CustomLargeImageUrl);
                break;
        }
    }
        
    private DateTime? _timerTime;
    private readonly Timer _timer;
    private void RpcClientOnPresenceUpdated(object? sender, PresenceMessage e) => this.RunUILogic(() =>
    {
        _timerTime = e.Presence.Timestamps?.Start;
        if (_timerTime != null)
        {
            _timer.Start();
        }
        else
        {
            _timer.Stop();
            tblTime.Text = string.Empty;
        }
            
        tblTitle.Text = e.Name;
        tblText1.Text = e.Presence.Details;
        tblText2.Text = e.Presence.State;
            
        if (!e.Presence.HasAssets())
        {
            brdLarge.IsVisible = false;
            gridSmallImage.IsVisible = false;
            CustomToolTip.SetTip(brdLarge, null);
            CustomToolTip.SetTip(gifLarge, null);
            CustomToolTip.SetTip(gridSmallImage, null);
            CustomToolTip.SetTip(gifSmallImage, null);
            return;
        }
        
        //Update key and asset
        var baseurl = "https://cdn.discordapp.com/app-assets/" + e.ApplicationID;
        CustomToolTip.SetTip(brdLarge, e.Presence.Assets.LargeImageText);
        CustomToolTip.SetTip(gifLarge, e.Presence.Assets.LargeImageText);
        CustomToolTip.SetTip(gridSmallImage, e.Presence.Assets.SmallImageText);
        CustomToolTip.SetTip(gifSmallImage, e.Presence.Assets.SmallImageText);
        _ = UpdateSmallImage(GetUri(e.Presence.Assets.SmallImageID, e.Presence.Assets.SmallImageKey, baseurl));
        _ = UpdateLargeImage(GetUri(e.Presence.Assets.LargeImageID, e.Presence.Assets.LargeImageKey, baseurl));
    });

    private Uri? GetUri(ulong? id, string key, string baseurl)
    {
        if (id.HasValue)
        {
            return new Uri(baseurl + "/" + id + ".png");
        }

        if (!string.IsNullOrWhiteSpace(key)
            && key.StartsWith("mp:external/")
            && Uri.TryCreate("https://media.discordapp.net/external/" + key[12..], UriKind.Absolute, out var url))
        {
            return url;
        }

        return null;
    }
        
    private void UpdateFromRichPresence(RichPresence? presence)
    {
        if (presence != null)
        {
            if (_rpcProfile != null)
            {
                _rpcProfile.PropertyChanged -= PresenceOnPropertyChanged;
            }
            _rpcProfile = presence;
        }
        if (_rpcProfile == null)
        {
            return;
        }

        //TODO: Make it so we can cancel this binding
        DoBinding(_rpcProfile.Profile, nameof(presence.Profile.Details), tblText1);
        DoBinding(_rpcProfile.Profile, nameof(presence.Profile.State), tblText2);
        _rpcProfile.PropertyChanged += PresenceOnPropertyChanged;
        _rpcProfile.Profile.PropertyChanged += ProfileOnPropertyChanged;
        ProfileOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(RpcProfile.Profile.LargeText)));
        ProfileOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(RpcProfile.Profile.SmallText)));
    }

    private async Task UpdateLargeImage(Uri? uri)
    {
        if (uri is null)
        {
            brdLarge.Background = null;
            brdLarge.IsVisible = false;
            gridSmallImage.IsVisible = false;
            gifLarge.IsVisible = false;
            gifLarge.Tag = null;
            gifLarge.SourceStream = Stream.Null;
            return;
        }

        if (await ProcessUri(uri))
        {
            gridSmallImage.IsVisible = ellSmallImage.Fill != null || gifSmallImage.Tag != null;
            if (CachedImages.ContainsKey(uri))
            {
                brdLarge.Background = CachedImages[uri];
                brdLarge.IsVisible = true;
                gifLarge.IsVisible = false;
                gifLarge.Tag = null;
                gifLarge.SourceStream = Stream.Null;
                return;
            }
            
            gifLarge.IsVisible = true;

            if (!gifLarge.Tag?.Equals(uri) ?? true)
            {
                gifLarge.Tag = uri;
                var memStream = new MemoryStream();
                lock (_streamLock)
                {
                    var stream = CachedStreams[uri];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                }

                gifLarge.SourceStream = memStream;
            }

            brdLarge.IsVisible = false;
        }
    }

    private readonly object _streamLock = new object();
    private async Task UpdateSmallImage(Uri? uri)
    {
        if (uri is null)
        {
            ellSmallImage.Fill = null;
            gifSmallImage.IsVisible = false;
            gifSmallImage.SourceStream = Stream.Null;
            gifSmallImage.Tag = null;
            gridSmallImage.IsVisible = false;
            return;
        }

        if (await ProcessUri(uri))
        {
            gridSmallImage.IsVisible = brdLarge.Background != null;
            if (CachedImages.ContainsKey(uri))
            {
                ellSmallImage.Fill = CachedImages[uri];
                ellSmallImage.IsVisible = true;
                gifSmallImage.IsVisible = false;
                gifSmallImage.SourceStream = Stream.Null;
                gifSmallImage.Tag = null;
                return;
            }
            
            gifSmallImage.IsVisible = true;
            if (!gifSmallImage.Tag?.Equals(uri) ?? true)
            {
                gifSmallImage.Tag = uri;
                var memStream = new MemoryStream();
                lock (_streamLock)
                {
                    var stream = CachedStreams[uri];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                }

                gifSmallImage.SourceStream = memStream;
            }
            ellSmallImage.IsVisible = false;
        }
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_timerTime.HasValue)
        {
            return;
        }
        var ts = DateTime.UtcNow.Subtract(_timerTime.Value);

        var text = ts.Hours > 0 ? ts.Hours.ToString("00") + ":" : string.Empty;
        text += $"{ts.Minutes:00}:{ts.Seconds:00}";
        this.RunUILogic(() => tblTime.Text = text);
    }

    private async Task<bool> ProcessUri(Uri uri)
    {
        if (CachedImages.ContainsKey(uri) 
            || CachedStreams.ContainsKey(uri))
        {
            return true;
        }

        var imageResponse = await App.HttpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get, uri));
        if (imageResponse is not { IsSuccessStatusCode: true })
        {
            return false;
        }

        var imageStream = await imageResponse.Content.ReadAsStreamAsync();
        var isGif = GifDecoder.IsGif(imageStream);
        imageStream.Seek(0, SeekOrigin.Begin);
        if (isGif)
        {
            lock (_streamLock)
            {
                CachedStreams[uri] = imageStream;
            }
            return true;
        }

        var image = Bitmap.DecodeToHeight(imageStream, (int)brdLarge.Height * 3);
        await imageStream.DisposeAsync();
        var brush = new ImageBrush(image);
        CachedImages[uri] = brush;
        return true;
    }

    private static void DoBinding(RpcProfile presence, string path, IAvaloniaObject control)
    {
        var binding = new Binding
        {
            Source = presence,
            Mode = BindingMode.OneWay,
            Path = path
        };
        control.Bind(TextBlock.TextProperty, binding);
    }

    public void UpdateBackground(IBrush brush)
    {
        brdContent.Background = brush;
    }
        
    private void UpdateFromType(string? error = null, RichPresence? richPresence = null)
    {
        tblText1.IsVisible = _viewType is not ViewType.Loading or ViewType.LocalRichPresence;
        tblText2.IsVisible = tblText1.IsVisible && _viewType is not ViewType.Error;
        tblTime.IsVisible = _viewType == ViewType.RpcRichPresence;
        gifLarge.IsVisible = _viewType == ViewType.Loading;
        if (!gifLarge.IsVisible)
        {
            gifLarge.SourceStream = Stream.Null;
            gifLarge.Tag = null;
        }

        var brush = _viewType switch
        {
            ViewType.Default => Application.Current.Resources["ThemeAccentBrush2"],
            ViewType.Default2 => Application.Current.Resources["ThemeAccentBrush2"],
            ViewType.Loading => Application.Current.Resources["ThemeAccentBrush2"],
            ViewType.Error => Application.Current.Resources["RedBrush"],
            ViewType.LocalRichPresence => Application.Current.Resources["PurpleBrush"],
            ViewType.RpcRichPresence => Application.Current.Resources["PurpleBrush"],
            _ => Application.Current.Resources["ThemeAccentBrush2"]
        };
        brdContent.Background = (IBrush)brush!;

        _rpcClient.PresenceUpdated -= RpcClientOnPresenceUpdated;
        switch (_viewType)
        {
            case ViewType.Default:
            {
                _titleText.ChangeJsonNames(LanguageText.MultiRPC);
                _tblText1.ChangeJsonNames(LanguageText.ThankYouForUsing);
                _tblText2.ChangeJsonNames(LanguageText.ThisProgram);
                brdLarge.Background = _logoVisualBrush;

                brdLarge.IsVisible = true;
                gridSmallImage.IsVisible = false;
                CustomToolTip.SetTip(brdLarge, null);
                CustomToolTip.SetTip(gifLarge, null);
            }
            break;
            case ViewType.Default2:
            {
                _titleText.ChangeJsonNames(LanguageText.MultiRPC);
                _tblText1.ChangeJsonNames(LanguageText.Hello);
                _tblText2.ChangeJsonNames(LanguageText.World);
                brdLarge.Background = _logoVisualBrush;

                brdLarge.IsVisible = true;
                gridSmallImage.IsVisible = false;
                CustomToolTip.SetTip(brdLarge, null);
                CustomToolTip.SetTip(gifLarge, null);
            }
            break;
            case ViewType.Loading:
            {
                gifLarge.SourceStream = AssetManager.GetSeekableStream("Loading.gif");
                gifLarge.Tag = null;

                _titleText.ChangeJsonNames(LanguageText.Loading);
                gridSmallImage.IsVisible = false;
                brdLarge.IsVisible = false;
            }
            break;
            case ViewType.Error:
            {
                _titleText.ChangeJsonNames(LanguageText.Error);

                tblTitle.Foreground = new ImmutableSolidColorBrush(Colors.White.ToUint32());
                tblText1.Text = error;
                tblText1.Foreground = tblTitle.Foreground;
                    
                brdLarge.Background = _errorVisualBrush;
                brdLarge.IsVisible = true;
                gridSmallImage.IsVisible = false;
                CustomToolTip.SetTip(brdLarge, null);
                CustomToolTip.SetTip(gifLarge, null);
            }
            break;
            case ViewType.LocalRichPresence:
            {
                UpdateFromRichPresence(richPresence);
            }
            break;
            case ViewType.RpcRichPresence:
            {
                _rpcClient.PresenceUpdated += RpcClientOnPresenceUpdated;
            }
            break;
        }
    }
}