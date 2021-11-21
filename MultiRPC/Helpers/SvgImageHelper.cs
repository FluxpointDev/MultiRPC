using System;
using Avalonia.Svg;

namespace MultiRPC.Helpers
{
    public static class SvgImageHelper
    {
        public static SvgImage LoadImage(string path, string basePath = "avares://MultiRPC/Assets/", Uri? baseUri = null)
        {
            return new SvgImage
            {
                Source = SvgSource.Load(basePath + path, baseUri)
            };
        }
    }
}