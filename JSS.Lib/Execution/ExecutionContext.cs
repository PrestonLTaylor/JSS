namespace JSS.Lib.Execution;

// 9.4 Execution Contexts, https://tc39.es/ecma262/#sec-execution-contexts
internal class ExecutionContext
{
    public ExecutionContext(Realm realm)
    {
        Realm = realm;
    }

    // FIXME: code evaluation state
    // FIXME: Function
    public Realm Realm { get; }
    // FIXME: ScriptOrModule
}
