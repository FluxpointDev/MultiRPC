using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Extensions;

public static class MainWindowExt
{
    public static bool TryClose<T>(this UserControl userControl, T result)
    {
        if (userControl.Parent?.Parent is MainWindow window)
        {
            window.Close(result);
            return true;
        }

        return false;
    }
        
    public static bool TryClose(this UserControl userControl)
    {
        if (userControl.Parent?.Parent is MainWindow window)
        {
            window.Close();
            return true;
        }

        return false;
    }
}