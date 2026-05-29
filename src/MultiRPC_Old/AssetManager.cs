using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Svg;
using Avalonia.Utilities;
using MultiRPC.Theming;
using ShimSkiaSharp;
using SM = Svg.Model;
using SP = Svg.Model;

namespace MultiRPC;

public static class AssetManager
{
    //Based on TODO: Find Git link
    private static readonly SM.IAssetLoader AssetLoader = new AvaloniaAssetLoader();
    private static readonly Dictionary<string, List<Action>> AssetReloadActions = new Dictionary<string, List<Action>>();
    private static readonly Dictionary<string, bool> LoadedAssets = new Dictionary<string, bool>();
    private static string[]? _knownAssets;

    public static readonly ImageFormat GifFormat = new ImageFormat("Gif", ".gif");
    public static readonly ImageFormat SvgFormat = new ImageFormat("Svg", ".svg");

    internal static event EventHandler? ReloadAssets;
    
    public static ImageFormat[] SupportedStaticAsset = new[]
    {
        SvgFormat,
        new ImageFormat("Png", ".png"),
        new ImageFormat("Webp", ".webp"),
        new ImageFormat("Wbmp", ".wbmp"),
        new ImageFormat("Jpeg", ".jpg", ".jpeg", ".jpe", ".jif", ".jfif"),
        new ImageFormat("Bmp", ".bmp", ".dib"),
        new ImageFormat("Ico", ".ico"),
        new ImageFormat("DNG", ".dng"),
        GifFormat,
    };
    public static ImageFormat[] SupportedAnimatedAsset = new[]
    {
        GifFormat,
    };
    

    private static IEnumerable<string> ReadResourcesFile()
    {
        var assembly = Assembly.GetEntryAssembly();
        using var resources = assembly?.GetManifestResourceStream("!AvaloniaResources");
        if (resources == null) yield break;

        var indexLength = new BinaryReader(resources).ReadInt32();
        var resourcesList = AvaloniaResourcesIndexReaderWriter.Read(new SlicedStream(resources, 4, indexLength));
        foreach (var resource in resourcesList)
        {
            if (resource.Path?.StartsWith("/Assets") ?? false)
            {
                yield return resource.Path;
            }
        }
    }
    
    internal static void FireReloadAssets(object? sender)
    {
        if (Theme.ActiveTheme == null)
        {
            ReloadAssets?.Invoke(sender, EventArgs.Empty);
            return;
        }
        
        foreach (var (key, value) in LoadedAssets)
        {
            var haveAsset = Theme.ActiveTheme.HaveAsset(key);
            if (AssetReloadActions.ContainsKey(key)
                && haveAsset != value || (haveAsset && value))
            {
                AssetReloadActions[key].ForEach(x => x.Invoke());
            }
        }
        
        ReloadAssets?.Invoke(sender, EventArgs.Empty);
    }

    public static string[]? GetAllAssets()
    {
        _knownAssets ??= ReadResourcesFile().ToArray();
        if (!_knownAssets.Any())
        {
            _knownAssets = null;
        }
        return _knownAssets;
    }

    public static void RegisterForAssetReload(string key, Action action)
    {
        if (!AssetReloadActions.ContainsKey(key))
        {
            AssetReloadActions[key] = new List<Action>();
        }
        AssetReloadActions[key].Add(action);
    }

    public static Stream GetSeekableStream(string key, Theme? theme = null)
    {
        var st = GetAsset(key, theme);
        if (st.CanSeek)
        {
            return st;
        }

        var mem = new MemoryStream();
        st.CopyTo(mem);
        mem.Seek(0, SeekOrigin.Begin);
        st.Dispose();
        return mem;
    }
    
    /// <summary>
    /// Grabs the stream for the asset
    /// </summary>
    public static Stream GetAsset(string key, Theme? theme = null)
    {
        var stream = Stream.Null;
        var fromDefaultTheme = theme == null;
        theme ??= Theme.ActiveTheme;
        if (theme?.HaveAsset(key) ?? false)
        {
            stream = theme.GetAssetStream(key);
            if (fromDefaultTheme)
            {
                LoadedAssets[key] = true;
            }
        }
        
        //If we wasn't able to load in from the theme, use default assets
        if (stream == Stream.Null)
        {
            var loader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            if (GetAllAssets() == null)
            {
                throw new Exception("Unable to get assets in the application");
            }

            var asset = GetAssetLocation(key);
            if (string.IsNullOrWhiteSpace(asset))
            {
                throw new Exception("Unable to find asset");
            }
            
            stream = loader.Open(new Uri("avares://MultiRPC" + asset, UriKind.RelativeOrAbsolute));
            if (fromDefaultTheme)
            {
                LoadedAssets[key] = false;
            }
        }
        
        return stream;
    }

    public static string? GetAssetLocation(string key)
    {
        return _knownAssets?.FirstOrDefault(asset => asset[8..].StartsWith(key));
    }

    public static SvgSource LoadSvgImage(string key, Theme? theme = null) => new SvgSource { Picture = LoadPicture(key, theme) };

    public static SKPicture? LoadPicture(string key, Theme? theme = null)
    {
        var stream = GetAsset(key, theme);
        if (stream == Stream.Null)
        {
            return default;
        }
        
        var document = SM.SvgExtensions.Open(stream);
        return document is { } ? 
            SM.SvgExtensions.ToModel(document, AssetLoader, out _, out _) 
            : default;
    }
}