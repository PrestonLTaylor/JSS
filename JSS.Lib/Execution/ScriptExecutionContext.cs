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
        if (env is null)
        {
            // NOTE: As we want to access the LexicalEnvironment there is an implicit assertion that
            // the execution context is a ScriptExecutionContext (as that's the only context that has a
            // lexical environment in the code base currently)
            // a. Set env to the running execution context's LexicalEnvironment.
            var scriptExecutionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
            env = scriptExecutionContext.LexicalEnvironment;
        }

        // NOTE: This Assert is implicit
        // 2. Assert: env is an Environment Record.

        // FIXME: 3. If the source text matched by the syntactic production that is being evaluated is contained in strict mode code,
        // let strict be true; else let strict be false.

        // 4. Return ? GetIdentifierReference(env, name, strict).
        return Environment.GetIdentifierReference(env, name);
    }

    public Environment? LexicalEnvironment { get; set; }
    public Environment? VariableEnvironment { get; set; }
    public Environment? PrivateEnvironment { get; set; }
}
