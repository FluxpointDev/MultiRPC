using MultiRPC.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MultiRPC.UWP.AssetProcessor
{
    /// <summary>
    /// Processor with a common <see cref="IAssetProcessor.ProcessAsset(Stream, object[])"/>, if you need to use a custom <see cref="IAssetProcessor.ProcessAsset(Stream, object[])"/> that can't be made by changing <see cref="MakeImageSource(Stream, object[])"/> then inheritance from <see cref="IAssetProcessor"/>
    /// </summary>
    class GenericAssetProcessor : IAssetProcessor
    {
        public virtual string AssetTarget => "N/A";

        public virtual Task<Stream> GetAsset(string assetPath, params object[] args) => throw new NotImplementedException();

        public async Task<object> ProcessAsset(Stream assetStream, params object[] args)
        {
            var image = new Image()
            {
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3),
            };

            image.Source = await MakeImageSource(assetStream, args);
            return image;
        }

        public async virtual Task<ImageSource> MakeImageSource(Stream assetStream, params object[] args)
        {
            // Set the SVG source to the selected file and give it the size of the button
            SvgImageSource svgImage = new SvgImageSource();
            if (args?.Length == 2)
            {
                svgImage.RasterizePixelHeight = (int)args[0];
                svgImage.RasterizePixelWidth = (int)args[1];
            }

            var s = assetStream.AsRandomAccessStream();
            await svgImage.SetSourceAsync(s).AsTask();

            return svgImage;
        }
    }
}
