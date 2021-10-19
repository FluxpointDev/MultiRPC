using System;
using System.Collections.Generic;
using MultiRPC.Rpc.Page;

namespace MultiRPC.UI.Pages
{
    public static class PageManager
    {
        private static readonly List<ISidePage> Pages = new List<ISidePage>();
        
        public static void AddPage(ISidePage page)
        {
            Pages.Add(page);
            PageAdded?.Invoke(page, page);
        }

        public static void AddRpcPage(RpcPage page)
        {
            RpcPageManager.AddPage(page);
            Pages.Add(page);
            PageAdded?.Invoke(page, page);
        }

        public static IReadOnlyList<ISidePage> CurrentPages => Pages.AsReadOnly();

        public static event EventHandler<ISidePage>? PageAdded;
    }
}