using JSS.Lib.AST.Values;
using JSS.Lib.Runtime;

namespace JSS.Lib.Execution;

// NOTE: This isn't required in the spec but is a helper for executing code
public sealed class VM
{
    internal VM(Realm realm)
    {
        Realm = realm;

        HostEnsureCanCompileStrings = (calleeRealm, parameterStrings, bodyString, direct) =>
        {
            // The default implementation of HostEnsureCanCompileStrings is to return NormalCompletion(UNUSED).
            return Empty.The;
        };
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

    internal void PushStrictness(bool isStrict)
    {
        _strictStack.Push(isStrict);
    }

    internal void PopStrictness()
    {
        _strictStack.Pop();
    }

    public Realm Realm { get; }

    // NOTE: Host defined functions are functions that have default behaviour but can be overriden by hosts that need to have different behaviours
    // 19.2.1.2 HostEnsureCanCompileStrings ( calleeRealm, parameterStrings, bodyString, direct ), https://tc39.es/ecma262/#sec-hostensurecancompilestrings
    internal Func<Realm, List<string>, string, bool, Completion> HostEnsureCanCompileStrings { get; set; }

    internal ObjectPrototype ObjectPrototype { get => Realm.ObjectPrototype; }
    internal ObjectConstructor ObjectConstructor { get => Realm.ObjectConstructor; }
    internal FunctionPrototype FunctionPrototype { get => Realm.FunctionPrototype; }
    internal ArrayPrototype ArrayPrototype { get => Realm.ArrayPrototype; }
    internal ErrorPrototype ErrorPrototype { get => Realm.ErrorPrototype; }

    internal NativeErrorConstructor EvalErrorConstructor { get => Realm.EvalErrorConstructor; }
    internal NativeErrorConstructor RangeErrorConstructor { get => Realm.RangeErrorConstructor; }
    internal NativeErrorConstructor ReferenceErrorConstructor { get => Realm.ReferenceErrorConstructor; }
    internal NativeErrorConstructor SyntaxErrorConstructor { get => Realm.SyntaxErrorConstructor; }
    internal NativeErrorConstructor TypeErrorConstructor { get => Realm.TypeErrorConstructor; }
    internal NativeErrorConstructor URIErrorConstructor { get => Realm.URIErrorConstructor; }

    internal ExecutionContext CurrentExecutionContext
    {
        get
        {
            return _executionContextStack.Peek();
        }
    }
    private readonly Stack<ExecutionContext> _executionContextStack = new();

    internal bool IsStrict
    {
        get
        {
            return _strictStack.Peek();
        }
    }
    private readonly Stack<bool> _strictStack = new();
}
