using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Extensions;

public static class MainWindowExt
{
    public static bool TryClose<T>(this IControl control, T result)
    {
        if (control.Parent?.Parent is MainWindow window)
        {
            window.Close(result);
            return true;
        }

        return false;
    }
        
    public static bool TryClose(this IControl control)
    {
        if (control.Parent?.Parent is MainWindow window)
        {
            window.Close();
            return true;
        }

        return false;
    }
}