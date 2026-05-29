using System.Diagnostics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaGif;
using AvaloniaGif.Decoding;

namespace MultiRPC.UI;

public class SwitchableImage : Control
{
    // ReSharper disable MemberCanBePrivate.Global
    public static readonly StyledProperty<Stream?> SourceStreamProperty =
        AvaloniaProperty.Register<SwitchableImage, Stream?>("SourceStream");

    public static readonly StyledProperty<IImage?> SourceImageProperty =
        AvaloniaProperty.Register<SwitchableImage, IImage?>("SourceImage");

    
    public static readonly StyledProperty<IterationCount> IterationCountProperty =
        AvaloniaProperty.Register<SwitchableImage, IterationCount>("IterationCount", IterationCount.Infinite);

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<SwitchableImage, Stretch>("Stretch");

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<SwitchableImage, StretchDirection>("StretchDirection");
    // ReSharper enable MemberCanBePrivate.Global

    private GifInstance? _gifInstance;
    private RenderTargetBitmap? _backingRtb;
    private Bitmap? _bitmap;
    private bool _hasNewSource;
    private Stream? _newSource;
    private Stopwatch? _stopwatch;

    static SwitchableImage()
    {
        IterationCountProperty.Changed.Subscribe(IterationCountChanged);
        SourceStreamProperty.Changed.Subscribe(SourceStreamChanged);
        SourceImageProperty.Changed.Subscribe(SourceImageChanged);

        AffectsRender<SwitchableImage>(SourceStreamProperty, SourceImageProperty, StretchProperty);
        AffectsArrange<SwitchableImage>(SourceStreamProperty, SourceImageProperty, StretchProperty);
        AffectsMeasure<SwitchableImage>(SourceStreamProperty, SourceImageProperty, StretchProperty);
    }

    public IterationCount IterationCount
    {
        get => GetValue(IterationCountProperty);
        set => SetValue(IterationCountProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }
    
    public Stream? SourceStream
    {
        get => GetValue(SourceStreamProperty);
        set => SetValue(SourceStreamProperty, value);
    }
    
    public IImage? SourceImage
    {
        get => GetValue(SourceImageProperty);
        set => SetValue(SourceImageProperty, value);
    }

    public void StopAndDispose()
    {
        _gifInstance?.Dispose();
        _backingRtb?.Dispose();
        _backingRtb = null;
        _bitmap = null;
    }

    public void ClearView()
    {
        StopAndDispose();
        SourceStream = null;
        SourceImage = null;
    }

    private static void IterationCountChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not SwitchableImage image || e.NewValue is not IterationCount iterationCount)
            return;

        image.IterationCount = iterationCount;
    }

    private static void SourceStreamChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not SwitchableImage image)
            return;

        var newVal = e.GetNewValue<Stream?>();
        if (newVal is null)
        {
            return;
        }

        image._hasNewSource = true;
        image._bitmap = null;
        image._newSource = newVal;
        image.SourceImage = null;
        Dispatcher.UIThread.Post(image.InvalidateVisual, DispatcherPriority.Background);
    }
    
    private static void SourceImageChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not SwitchableImage image)
            return;
        
        var newVal = e.GetNewValue<IImage?>();
        if (newVal is null)
        {
            return;
        }

        image._hasNewSource = true;
        image._bitmap = null;
        image._newSource = null;
        image.SourceStream = null;
        Dispatcher.UIThread.Post(image.InvalidateVisual, DispatcherPriority.Background);
    }

    public override void Render(DrawingContext context)
    {
        Dispatcher.UIThread.Post(InvalidateMeasure, DispatcherPriority.Background);
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

        if (_hasNewSource)
        {
            StopAndDispose();
            if (GifDecoder.IsGif(_newSource))
            {
                _gifInstance = new GifInstance(_newSource);
                _newSource = null;
                _gifInstance.IterationCount = IterationCount;
                _backingRtb = new RenderTargetBitmap(_gifInstance.GifPixelSize, new Vector(96, 96));

                _stopwatch ??= Stopwatch.StartNew();
            }
            else if (_newSource is not null)
            {
                _newSource.Seek(0, SeekOrigin.Begin);
                _bitmap = new Bitmap(_newSource);
                _newSource = null;
            }

            _hasNewSource = false;
            return;
        }

        ProcessGif(context);

        var image = GetImage();
        if (image is not null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            var viewPort = new Rect(Bounds.Size);
            var sourceSize = image.Size;

            var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
            var scaledSize = sourceSize * scale;
            var destRect = viewPort
                .CenterRect(new Rect(scaledSize))
                .Intersect(viewPort);

            var sourceRect = new Rect(sourceSize)
                .CenterRect(new Rect(destRect.Size / scale));

            var interpolationMode = RenderOptions.GetBitmapInterpolationMode(this);

            context.DrawImage(image, sourceRect, destRect, interpolationMode);
        }
    }

    private IImage? GetImage() => _backingRtb ?? _bitmap ?? SourceImage;
    
    private void ProcessGif(DrawingContext context)
    {
        if (_gifInstance is null || (_gifInstance.CurrentCts?.IsCancellationRequested ?? true))
        {
            _backingRtb = null;
            return;
        }

        if (!_stopwatch!.IsRunning)
        {
            _stopwatch.Start();
        }

        var currentFrame = _gifInstance.ProcessFrameTime(_stopwatch.Elapsed);

        if (currentFrame is { } && _backingRtb is { })
        {
            using var ctx = _backingRtb.CreateDrawingContext(null);
            var ts = new Rect(currentFrame.Size);
            ctx.DrawBitmap(currentFrame.PlatformImpl, 1, ts, ts);
            //return true;
        }

        //return false;
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = GetImage();
        var result = new Size();

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);
        }

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = GetImage();

        if (source != null)
        {
            var sourceSize = source.Size;
            var result = Stretch.CalculateSize(finalSize, sourceSize);
            return result;
        }

        return new Size();
    }
}