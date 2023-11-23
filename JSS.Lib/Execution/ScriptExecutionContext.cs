namespace JSS.Lib.Execution;

internal sealed class ScriptExecutionContext : ExecutionContext
{
    public ScriptExecutionContext(Realm realm) : base(realm)
    {
    }

    // FIXME: LexicalEnvironment
    // FIXME: VariableEnvironment 
    // FIXME: PrivateEnvironment 
}
