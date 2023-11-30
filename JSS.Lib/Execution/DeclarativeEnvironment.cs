namespace JSS.Lib.Execution;

// 9.1.1.1 Declarative Environment Records, https://tc39.es/ecma262/#sec-declarative-environment-records
internal sealed class DeclarativeEnvironment : Environment
{
    public DeclarativeEnvironment(Environment outerEnv) : base(outerEnv)
    {
    }

    // 9.1.1.1.1 HasBinding ( N ), https://tc39.es/ecma262/#sec-declarative-environment-records
    override public bool HasBinding(string N)
    {
        // 1. If envRec has a binding for N, return true.
        // 2. Return false.
        return _identifierToBinding.ContainsKey(N);
    }

    private readonly Dictionary<string, Binding> _identifierToBinding = new();
}
