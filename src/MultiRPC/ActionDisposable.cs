namespace MultiRPC;

public class ActionDisposable : IDisposable
{
    private readonly Action _disposeAction;
    public ActionDisposable(Action action, Action disposeAction)
    {
        Action = action;
        _disposeAction = disposeAction;
    }

    public Action Action { get; }

    public void Dispose()
    {
        _disposeAction.Invoke();
        GC.SuppressFinalize(this);
    }
}

public class ActionDisposable<T> : IDisposable
{
    private readonly Action<T?> _disposeAction;
    public ActionDisposable(Action<T?> action, Action<T?> disposeAction, T? state)
    {
        Action = action;
        _disposeAction = disposeAction;
        State = state;
    }

    public T? State { get; }

    public Action<T?> Action { get; }

    public void RunAction() => Action.Invoke(State);
    
    public void Dispose()
    {
        _disposeAction.Invoke(State);
        GC.SuppressFinalize(this);
    }
}