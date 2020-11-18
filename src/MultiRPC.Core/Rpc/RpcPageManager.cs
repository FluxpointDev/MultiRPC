using Microsoft.Extensions.DependencyInjection;
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
            foreach (var page in ServiceManager.ServiceProvider.GetServices<IRpcPage>())
            {
                page.Accessed += (sender,__) =>
                {
                    if (CurrentPage?.RichPresence != null)
                    {
                        CurrentPage.RichPresence.PropertyChanged -= RichPresence_PropertyChanged;
                    }
                    page.RichPresence.PropertyChanged += RichPresence_PropertyChanged;
                    //TODO: add check to see if we are already running the imp RpcClient, if this is the case then call
                    //NewCurrentPage when it stops and if it is still the "current page"
                    CurrentPage = page;
                    NewCurrentPage?.Invoke(sender, page);
                };
            }
        }

        private static void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO: Add a timer to auto update rich presence
            //throw new NotImplementedException();
        }

        public static IRpcPage CurrentPage { get; private set; }

        public static event EventHandler<IRpcPage> NewCurrentPage;
    }
}
