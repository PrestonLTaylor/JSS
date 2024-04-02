namespace JSS.Lib.Execution;

// NOTE: This isn't required in the spec but is a helper for executing code
internal sealed class VM
{
    public VM(Realm realm)
    {
        Realm = realm;
    }

    public void PushExecutionContext(ExecutionContext context)
    {
        _executionContextStack.Push(context);
    }

    public void PopExecutionContext()
    {
        _executionContextStack.Pop();
    }

    public bool HasExecutionContext()
    {
        return _executionContextStack.Count > 0;
    }

    public Realm Realm { get; }

    public ExecutionContext CurrentExecutionContext
    {
        get
        {
            return _executionContextStack.Peek();
        }
    }
    private readonly Stack<ExecutionContext> _executionContextStack = new();
}
