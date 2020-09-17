using MultiRPC.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace MultiRPC.UWP.AssetProcessor
{
    class GifProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Gif";

        public async override Task<Stream> GetAsset(string assetPath, params object[] args)
        {
            assetPath = assetPath.Remove(0, AssetTarget.Length + 1);
            var fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();

            var s = await fileSystem.GetFileStream($"Assets/{assetPath}.gif");
            return s;
        }

        public async override Task<ImageSource> MakeImageSource(Stream assetStream, params object[] args)
        {
            // Set the SVG source to the selected file and give it the size of the button
            BitmapImage bitmapImage = new BitmapImage();
            var s = assetStream.AsRandomAccessStream();
            await bitmapImage.SetSourceAsync(s).AsTask();

            return bitmapImage;
        }
    }
}
