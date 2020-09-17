using MultiRPC.Core;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MultiRPC.UWP.AssetProcessor
{
    class IconProcessor : GenericAssetProcessor
    {
        public override string AssetTarget => "Icon";

        public override async Task<Stream> GetAsset(string assetPath, params object[] args)
        {
            assetPath = assetPath.Remove(0, AssetTarget.Length + 1);
            var fileSystem = ServiceManager.ServiceProvider.GetService<IFileSystemAccess>();

            var s = await fileSystem.GetFileStream($"Assets/{assetPath}Icon.svg");
            return s;
        }
    }
}
