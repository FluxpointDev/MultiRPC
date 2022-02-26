using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Extensions;

public static class WindowExt
{
    public static Task ShowDialog(this Window window)
    {
        return window.ShowDialog(App.MainWindow);
    }
    
    public static Task<string[]?> ShowAsync(this OpenFileDialog window)
    {
        return window.ShowAsync(App.MainWindow);
    }
}