using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using MultiRPC.AssetProcessor;

namespace MultiRPC.Common.AssetProcessor
{
    class PageIconProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Page";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args) =>
            await FileSystem.GetFileStreamAsync($"Assets/{assetPath.Remove(0, AssetTarget.Length + 1)}Icon.{DefaultExtension}");
    }
}
