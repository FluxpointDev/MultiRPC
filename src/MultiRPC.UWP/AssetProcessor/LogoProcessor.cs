using MultiRPC.Core;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MultiRPC.UWP.AssetProcessor
{
    class LogoProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon/Logo";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args)
        {
            var fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();

            var s = await fileSystem.GetFileStream($"Assets/MultiRPCLogo.svg");
            return s;
        }
    }
}
