using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace MultiRPC.Core
{
    public static class AssetManager
    {
        public async static Task<object> GetAsset(string assetPath, params object[] args)
        {
            var ogAssetPath = assetPath;
            var assetProcessors = ServiceManager.ServiceProvider.GetServices<IAssetProcessor>();
            while (assetProcessors.FirstOrDefault(x => x.AssetTarget == assetPath) == null)
            {
                var slashIndex = assetPath.LastIndexOf('/');
                if (slashIndex == -1)
                {
                    throw new Exception($"There is no {nameof(IAssetProcessor)} capable of processing {ogAssetPath}");
                }

                assetPath = assetPath.Remove(slashIndex);
            }
            var processor = assetProcessors.First(x => x.AssetTarget == assetPath);
            var assetStream = await processor.GetAsset(ogAssetPath, args);
            if (assetStream == Stream.Null)
            {
                Serilog.Log.Logger.Error("Unable to get asset stream");
                return null;
            }

            return await processor.ProcessAsset(assetStream, args);
        }
    }
}
