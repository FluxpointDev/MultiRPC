using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Extensions;

public static class WindowExt
{
    public static Task ShowDialog(this Window window)
    {
        return window.ShowDialog(App.MainWindow);
    }
}