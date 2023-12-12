using JSS.Lib.AST.Values;

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
    public Empty Empty { get; } = new Empty();
    public Null Null { get; } = new Null();
    public Undefined Undefined { get; } = new Undefined();
    public Number NaN { get; } = new Number(double.NaN);

    public ExecutionContext CurrentExecutionContext
    {
        get
        {
            return _executionContextStack.Peek();
        }
    }

    private Stack<ExecutionContext> _executionContextStack = new();
}
