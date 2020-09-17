using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MultiRPC.UWP.AssetProcessor
{
    class PageIconProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Page";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args)
        {
            //TODO: Add something to get the "selected" icon (maybe mess with this stream?)
            assetPath = assetPath.Remove(0, AssetTarget.Length + 1);
            var fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();

            var s = await fileSystem.GetFileStream($"Assets/{assetPath}Icon.svg");
            return s;
        }
    }
}
