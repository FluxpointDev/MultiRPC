using MultiRPC.AssetProcessor;
using MultiRPC.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MultiRPC.UWP.AssetProcessor
{
    class GifProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Gif";

        public async override Task<Stream> GetAsset(string assetPath, params object[] args) =>
            await FileSystem.GetFileStreamAsync($"Assets/{assetPath.Remove(0, AssetTarget.Length + 1)}.gif");

        public async override Task<ImageSource> MakeImageSource(Stream assetStream, params object[] args)
        {
            BitmapImage bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(assetStream.AsRandomAccessStream());

            return bitmapImage;
        }
    }
}
