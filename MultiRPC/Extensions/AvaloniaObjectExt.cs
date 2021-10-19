using System;
using Avalonia;
using Avalonia.Threading;

namespace MultiRPC.Extensions
{
    public static class AvaloniaObjectExt
    {
        public static void RunUILogic(this AvaloniaObject avaloniaObject, Action action)
        {
            if (!avaloniaObject.CheckAccess())
            {
                Dispatcher.UIThread.Post(action);
                return;
            }
            
            action.Invoke();
        }
    }
}