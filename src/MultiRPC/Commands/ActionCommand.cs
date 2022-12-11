using System.Windows.Input;

namespace MultiRPC.Commands;

public class ActionCommand : ICommand
{
    private readonly Action? _action;
    private readonly Action<object?>? _parameterAction;
    public ActionCommand(Action action)
    {
        _action = action;
    }
    public ActionCommand(Action<object?> action)
    {
        _parameterAction = action;
    }
    
    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        if (_parameterAction != null)
        {
            _parameterAction.Invoke(parameter);
            return;
        }
        
        _action?.Invoke();
    }

    public event EventHandler? CanExecuteChanged;
}