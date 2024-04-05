namespace JSS.Lib.Execution;

// NOTE: This isn't required in the spec but is a helper for executing code
public sealed class VM
{
    internal VM(Realm realm)
    {
        Realm = realm;
    }

    internal void PushExecutionContext(ExecutionContext context)
    {
        _executionContextStack.Push(context);
    }

    internal void PopExecutionContext()
    {
        _executionContextStack.Pop();
    }

    internal bool HasExecutionContext()
    {
        return _executionContextStack.Count > 0;
    }

    public Realm Realm { get; }

    internal ExecutionContext CurrentExecutionContext
    {
        get
        {
            return _executionContextStack.Peek();
        }
    }
    private readonly Stack<ExecutionContext> _executionContextStack = new();
}
