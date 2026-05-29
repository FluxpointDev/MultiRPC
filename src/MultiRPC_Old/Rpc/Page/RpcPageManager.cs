using MultiRPC.Exceptions;
using Splat;

namespace MultiRPC.Rpc.Page;

public static class RpcPageManager
{
    public static IRpcPage? CurrentPage { get; private set; }
    public static IRpcPage? PendingPage { get; private set; }

    private static RpcClient? _rpcClient;

    private static void RpcClient_Disconnected(object? sender, EventArgs e)
    {
        if (PendingPage == null)
        {
            return;
        }

        CurrentPage = PendingPage;
        PendingPage = null;
        NewCurrentPage?.Invoke(sender, CurrentPage);
        PageChanged?.Invoke(sender, CurrentPage);
    }
        
    internal static void NewActivePage(IRpcPage page)
    {
        if (_rpcClient == null)
        {
            _rpcClient = Locator.Current.GetService<RpcClient>() ?? throw new NoRpcClientException();
            _rpcClient.Disconnected += RpcClient_Disconnected;
        }
            
        if (_rpcClient.IsRunning)
        {
            PendingPage = page;
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
    public static event EventHandler<IRpcPage>? NewCurrentPage;

    /// <summary>
    /// This is when we go to a new page (Will also get called with NewCurrentPage)
    /// </summary>
    public static event EventHandler<IRpcPage>? PageChanged;
}