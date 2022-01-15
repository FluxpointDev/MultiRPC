using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Extensions;

public static class WindowExt
{
    public static Task ShowDialog(this Window window)
    {
        return window.ShowDialog(GetMainWindow);
    }
    
    public static Task<string[]?> ShowAsync(this OpenFileDialog window)
    {
        return window.ShowAsync(GetMainWindow);
    }
    
    public static Window GetMainWindow => ((App)Application.Current).DesktopLifetime?.MainWindow;
}