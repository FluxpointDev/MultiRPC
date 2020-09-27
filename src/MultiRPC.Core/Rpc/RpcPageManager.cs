using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiRPC.Core.Rpc
{
    public static class RpcPageManager
    {
        private static IRpcClient RpcClient;

        /// <summary>
        /// This load up everything that we nee
        /// </summary>
        public static void Load()
        {
            RpcClient = ServiceManager.ServiceProvider.GetService<IRpcClient>();
            foreach (var page in ServiceManager.ServiceProvider.GetServices<IRpcPage>())
            {
                page.Accessed += (sender,__) => 
                {
                    //TODO: add check to see if we are already running the imp RpcClient, if this is the case then call
                    //NewCurrentPage when it stops and if it is still the "current page"
                    CurrentPage = page;
                    NewCurrentPage?.Invoke(sender, page);
                };
            }
        }

        public static IRpcPage CurrentPage { get; private set; }

        public static event EventHandler<IRpcPage> NewCurrentPage;
    }
}
