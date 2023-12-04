namespace JSS.Lib.Execution;

internal sealed class ScriptExecutionContext : ExecutionContext
{
    public ScriptExecutionContext(Realm realm) : base(realm)
    {
    }

    // 9.4.2 ResolveBinding ( name [ , env ] ), https://tc39.es/ecma262/#sec-resolvebinding
    static public Completion ResolveBinding(VM vm, string name, Environment? env = null)
    {
        // 1. If env is not present or env is undefined, then
        // FIXME: a. Set env to the running execution context's LexicalEnvironment.
        env ??= GlobalEnvironment;

        // NOTE: This Assert is implicit
        // 2. Assert: env is an Environment Record.

        // FIXME: 3. If the source text matched by the syntactic production that is being evaluated is contained in strict mode code,
        // let strict be true; else let strict be false.

        // 4. Return ? GetIdentifierReference(env, name, strict).
        return Environment.GetIdentifierReference(vm, env, name);
    }

    static public GlobalEnvironment GlobalEnvironment { get; } = new(new(null), new(null));

    // FIXME: LexicalEnvironment

    // FIXME: VariableEnvironment 

    // FIXME: PrivateEnvironment 
}
