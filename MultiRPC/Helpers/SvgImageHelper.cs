using Avalonia.Svg;

namespace MultiRPC.Helpers;

public static class SvgImageHelper
{
    public static SvgImage LoadImage(string path)
    {
        return new SvgImage
        {
            Source = AssetManager.LoadSvgImage(path)
        };
    }
}