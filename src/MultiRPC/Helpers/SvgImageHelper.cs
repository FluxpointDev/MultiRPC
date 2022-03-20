using Avalonia.Svg;
using MultiRPC.Theming;

namespace MultiRPC.Helpers;

public static class SvgImageHelper
{
    public static SvgImage LoadImage(string path, Theme? theme = null)
    {
        return new SvgImage
        {
            Source = AssetManager.LoadSvgImage(path, theme)
        };
    }
}