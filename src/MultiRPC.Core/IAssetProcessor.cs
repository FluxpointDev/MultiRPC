using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MultiRPC.Core
{
    //TODO: See if this is still needed
    public interface IAssetProcessor : IRequired
    {
        /// <summary>
        /// What this would process
        /// </summary>
        string AssetTarget { get; }

        /// <summary>
        /// Gets the asset stream for processing
        /// </summary>
        /// <param name="args">any arguments that need to be passed to get the asset</param>
        /// <returns></returns>
        Task<Stream> GetAsset(string assetPath, params object[] args);

        /// <summary>
        /// Processes the asset into what it needs to be
        /// </summary>
        /// <param name="assetStream"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<object> ProcessAsset(Stream assetStream, params object[] args);
    }
}
