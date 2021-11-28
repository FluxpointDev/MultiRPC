using System;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Svg;
using MultiRPC.Theming;
using ShimSkiaSharp;
using SM = Svg.Model;
using SP = Svg.Model;

namespace MultiRPC;
public static class AssetManager
{
    internal static event EventHandler? ReloadAssets;

    internal static void FireReloadAssets(object? sender) => ReloadAssets?.Invoke(sender, EventArgs.Empty);

    /// <summary>
    /// Grabs
    /// </summary>
    /// <param name="key"></param>
    /// <param name="theme"></param>
    /// <returns></returns>
    public static Stream GetAsset(string key, Theme? theme = null)
    {
        var stream = Stream.Null;
        theme ??= Theme.ActiveTheme;
        if (theme?.HaveAsset(key) ?? false)
        {
            stream = theme.GetAssetStream(key);
        }
        
        //If we wasn't able to load in from the theme, use default assets
        if (stream == Stream.Null)
        {
            var loader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            stream = loader.Open(new Uri("avares://MultiRPC/Assets/" + key, UriKind.RelativeOrAbsolute));
        }
        
        return stream;
    }

    //Based on TODO: Find Git link
    private static readonly SM.IAssetLoader AssetLoader = new AvaloniaAssetLoader();
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