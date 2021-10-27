using System;
using Avalonia.Controls;
using Avalonia.Svg;

namespace MultiRPC
{
    public class SvgImageHelper
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