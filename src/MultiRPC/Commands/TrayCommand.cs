using System.Windows.Input;
using Avalonia.Controls;
using MultiRPC.UI;

namespace MultiRPC.Commands;

internal class TrayCommand : ICommand
{
    public bool CanExecute(object? _) => true;

    public void Execute(object? _)
    {
        var mainWin = App.MainWindow;
        switch (mainWin.WindowState)
        {
            case WindowState.Normal:
            case WindowState.Maximized:
            case WindowState.FullScreen:
                mainWin.WindowState = WindowState.Minimized;
                break;
            case WindowState.Minimized:
                mainWin.WindowState = WindowState.Normal;
                return;
            default:
                return;
        }
    }

    public event EventHandler? CanExecuteChanged;
}