using Avalonia.Controls;
using MultiRPC.Helpers;

namespace MultiRPC.Extensions;

public static class SvgExt
{
    public static void AddSvgAsset(this Image image, string key)
    {
        image.Source = SvgImageHelper.LoadImage(key);
        AssetManager.RegisterForAssetReload(key, () => image.Source = SvgImageHelper.LoadImage(key));
    }
    
    public static void AddSvgAsset(this Button btn, string key)
    {
        if (btn.Content is not Image image)
        {
            btn.Content = image = new Image();
        }
        image.AddSvgAsset(key);
    }
}