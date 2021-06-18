using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiRPC
{
    /// <summary>
    /// Small class to run code on the Dispatcher based on the UI Framework being used
    /// </summary>
    public static class DispatcherHelper
    {
        public static void RunLogic(this UserControl control, Action action)
        {
#if !UNO
            control.DispatcherQueue.TryEnqueue(Microsoft.System.DispatcherQueuePriority.High, new Microsoft.System.DispatcherQueueHandler(action));
#else
            _ = control.Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.High, new Windows.UI.Core.DispatchedHandler(action));
#endif
        }
    }
}
