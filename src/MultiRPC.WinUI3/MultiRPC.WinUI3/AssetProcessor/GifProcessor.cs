using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MultiRPC.AssetProcessor;

namespace MultiRPC.WinUI3.AssetProcessor
{
    class GifProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Gif";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args) =>
            await FileSystem.GetFileStreamAsync($"Assets/{assetPath.Remove(0, AssetTarget.Length + 1)}.gif");

        public async override Task<ImageSource> MakeImageSource(Stream assetStream, params object[] args)
        {
            BitmapImage bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(assetStream.AsRandomAccessStream()).AsTask();

            return bitmapImage;
        }
    }
}
