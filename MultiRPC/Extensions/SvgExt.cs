using Avalonia.Controls;
using Avalonia.Svg;
using MultiRPC.Helpers;

namespace MultiRPC.Extensions;

public static class SvgExt
{
    public static void AddSvgAsset(this Image image, string key)
    {
        image.Source = SvgImageHelper.LoadImage(key);
        AssetManager.RegisterForAssetReload(key, () => image.Source = SvgImageHelper.LoadImage(key));
    }
}