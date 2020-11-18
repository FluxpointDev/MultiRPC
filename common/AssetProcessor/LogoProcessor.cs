using MultiRPC.Core;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.Storage;
using System.Diagnostics;
using MultiRPC.AssetProcessor;

namespace MultiRPC.Common.AssetProcessor
{
    class LogoProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Logo";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args) =>
            await FileSystem.GetFileStreamAsync($"Assets/MultiRPCLogo.{DefaultExtension}");
    }
}
