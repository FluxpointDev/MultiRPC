using System;

namespace MultiRPC.Rpc.Page
{
    public static class RpcPageManager
    {
        public static void GiveRpcClient(RpcClient rpcClient)
        {
            rpcClient.Disconnected += RpcClient_Disconnected;
            _rpcClient = rpcClient;
        }

        public static RpcPage? CurrentPage { get; private set; }

        private static RpcClient _rpcClient = null!;
        private static RpcPage? _pendingPage;
        private static void RpcClient_Disconnected(object? sender, EventArgs e)
        {
            if (_pendingPage == null)
            {
                return;
            }

            CurrentPage = _pendingPage;
            _pendingPage = null;
            NewCurrentPage?.Invoke(sender, CurrentPage);
            PageChanged?.Invoke(sender, CurrentPage);
        }
        
        internal static void PageMoved(RpcPage page)
        {
            if (_rpcClient.IsRunning)
            {
                _pendingPage = page;
                PageChanged?.Invoke(null, page);
                return;
            }

            CurrentPage = page;
            NewCurrentPage?.Invoke(null, page);
            PageChanged?.Invoke(null, page);
        }

        /// <summary>
        /// This is when we are actively tracking the Rich Presence from a new page
        /// </summary>
        public static event EventHandler<RpcPage>? NewCurrentPage;

        /// <summary>
        /// This is when we go to a new page (Will also get called with NewCurrentPage)
        /// </summary>
        public static event EventHandler<RpcPage>? PageChanged;
    }
}