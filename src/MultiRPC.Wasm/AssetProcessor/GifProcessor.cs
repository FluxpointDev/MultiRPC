using MultiRPC.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Windows.Storage;
using MultiRPC.AssetProcessor;

namespace MultiRPC.Wasm.AssetProcessor
{
    class GifProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Gif";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args) =>
            await FileSystem.GetFileStreamAsync($"Assets/{assetPath.Remove(0, AssetTarget.Length + 1)}.gif");
    }
}
