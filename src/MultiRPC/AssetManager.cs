using System;
using System.Collections.Generic;
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
    //Based on TODO: Find Git link
    private static readonly SM.IAssetLoader AssetLoader = new AvaloniaAssetLoader();
    private static readonly Dictionary<string, List<Action>> AssetReloadActions = new Dictionary<string, List<Action>>();
    private static readonly Dictionary<string, bool> LoadedAssets = new Dictionary<string, bool>();

    internal static event EventHandler? ReloadAssets;
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

    public static void RegisterForAssetReload(string key, Action action)
    {
        if (!AssetReloadActions.ContainsKey(key))
        {
            AssetReloadActions[key] = new List<Action>();
        }
        AssetReloadActions[key].Add(action);
    }

    public static Stream GetSeekableStream(string key)
    {
        var st = GetAsset(key);
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
            stream = loader.Open(new Uri("avares://MultiRPC/Assets/" + key, UriKind.RelativeOrAbsolute));
            if (fromDefaultTheme)
            {
                LoadedAssets[key] = false;
            }
        }
        
        return stream;
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