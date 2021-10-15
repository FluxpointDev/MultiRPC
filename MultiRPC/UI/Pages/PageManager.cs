using System;
using System.Collections.Generic;
using Avalonia.Controls;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages
{
    public static class PageManager
    {
        private static readonly List<SidePage> Pages = new List<SidePage>();
        
        public static void AddPage(SidePage page)
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

        public static IReadOnlyList<SidePage> CurrentPages => Pages.AsReadOnly();

        public static event EventHandler<SidePage>? PageAdded;
    }
}