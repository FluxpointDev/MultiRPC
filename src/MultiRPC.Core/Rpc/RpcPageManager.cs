using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core.Pages;
using System;
using System.ComponentModel;

namespace MultiRPC.Core.Rpc
{
    public static class RpcPageManager
    {
        private static RpcClient RpcClient;

        /// <summary>
        /// This load up everything that we need
        /// </summary>
        public static void Load()
        {
            if (ServiceManager.ServiceProvider == null)
            {
                throw new Exception(LanguagePicker.GetLineFromLanguageFile("ServiceProviderNotLoaded"));
            }
            
            RpcClient = ServiceManager.ServiceProvider.GetRequiredService<RpcClient>();
            RpcClient.Disconnected += RpcClient_Disconnected;
            foreach (var page in ServiceManager.ServiceProvider.GetServices<IRpcPage>())
            {
                page.Accessed += (sender,__) =>
                {
                    if (CurrentPage != null)
                    {
                        CurrentPage.PropertyChanged -= RichPresence_PropertyChanged;
                        CurrentPage.PropertyChanged -= OnPagePropertyChanged;
                        page.PropertyChanged += RichPresence_PropertyChanged;
                        page.PropertyChanged += OnPagePropertyChanged;
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

        private static void RpcClient_Disconnected(object sender, EventArgs e)
        {
            if (RpcPageStore == null)
            {
                return;
            }

            CurrentPage = RpcPageStore;
            RpcPageStore = null;
            NewCurrentPage?.Invoke(sender, RpcPageStore);
            PageChanged?.Invoke(sender, RpcPageStore);
        }

        private static void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e) => 
            PagePropertyChanged?.Invoke(sender, e);

        private static void RichPresence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO: Add a timer to auto update rich presence
            //throw new NotImplementedException();
        }

        //So we can keep track of the page that we need to get content from once we have stopped the connection
        private static IRpcPage? RpcPageStore;

        public static IRpcPage? CurrentPage { get; private set; }

        public static event PropertyChangedEventHandler? PagePropertyChanged;

        /// <summary>
        /// This is when we go to a new page that we are tracking for Rich Pre
        /// </summary>
        public static event EventHandler<IRpcPage>? NewCurrentPage;

        /// <summary>
        /// This is when we go to a new page (Will also get called with NewCurrentPage)
        /// </summary>
        public static event EventHandler<IRpcPage>? PageChanged;
    }
}
