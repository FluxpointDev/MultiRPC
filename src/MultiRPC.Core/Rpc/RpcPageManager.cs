using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core.Page;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MultiRPC.Core.Rpc
{
    public static class RpcPageManager
    {
        private static IRpcClient RpcClient;

        /// <summary>
        /// This load up everything that we need
        /// </summary>
        public static void Load()
        {
            RpcClient = ServiceManager.ServiceProvider.GetService<IRpcClient>();
            RpcClient.Disconnected += RpcClient_Disconnected;
            foreach (var page in ServiceManager.ServiceProvider.GetServices<IRpcPage>())
            {
                page.Accessed += (sender,__) =>
                {
                    if (CurrentPage?.RichPresence != null)
                    {
                        CurrentPage.RichPresence.PropertyChanged -= RichPresence_PropertyChanged;
                    }
                    if (page?.RichPresence != null)
                    {
                        page.RichPresence.PropertyChanged += RichPresence_PropertyChanged;
                    }

                    if (RpcClient.IsRunning)
                    {
                        RpcPageStore = page;
                        PageChanged?.Invoke(sender, page);
                        return;
                    }

                    CurrentPage = page;
                    NewCurrentPage?.Invoke(sender, page);
                    PageChanged?.Invoke(sender, page);
                };
            }
        }

        private static void RpcClient_Disconnected(object sender, bool e)
        {
            if (RpcPageStore == null)
            {
                return;
            }

            CurrentPage = RpcPageStore;
            NewCurrentPage?.Invoke(sender, RpcPageStore);
            PageChanged?.Invoke(sender, RpcPageStore);
            RpcPageStore = null;
        }

        private static void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO: Add a timer to auto update rich presence
            //throw new NotImplementedException();
        }

        //So we can keep track of the page that we need to get content from once we have stopped the connection
        private static IRpcPage RpcPageStore;

        public static IRpcPage CurrentPage { get; private set; }

        /// <summary>
        /// This is when we go to a new page that we are tracking for Rich Pre
        /// </summary>
        public static event EventHandler<IRpcPage> NewCurrentPage;

        /// <summary>
        /// This is when we go to a new page (Will also get called with NewCurrentPage)
        /// </summary>
        public static event EventHandler<IRpcPage> PageChanged;
    }
}
