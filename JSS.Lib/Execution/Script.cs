using JSS.Lib.AST;

namespace JSS.Lib.Execution;

// 16.1.4 Script Records, https://tc39.es/ecma262/#sec-script-records 
internal sealed class Script
{
    public Script(VM vm, StatementList statementList)
    {
        VM = vm;
        Realm = vm.Realm;
        _statementList = statementList;
    }

    // 16.1.6 ScriptEvaluation ( scriptRecord ), https://tc39.es/ecma262/#sec-runtime-semantics-scriptevaluation
    public Completion ScriptEvaluation()
    {
        // FIXME: 1. Let globalEnv be scriptRecord.[[Realm]].[[GlobalEnv]].

        // 2. Let scriptContext be a new ECMAScript code execution context.
        // FIXME: 3. Set the Function of scriptContext to null.
        // 4. Set the Realm of scriptContext to scriptRecord.[[Realm]].
        // FIXME: 5. Set the ScriptOrModule of scriptContext to scriptRecord.
        // FIXME: 6. Set the VariableEnvironment of scriptContext to globalEnv.
        // FIXME: 7. Set the LexicalEnvironment of scriptContext to globalEnv.
        // FIXME: 8. Set the PrivateEnvironment of scriptContext to null.
        var _ = new ScriptExecutionContext(Realm);

        // FIXME: 9. Suspend the running execution context.
        // FIXME: 10. Push scriptContext onto the execution context stack; scriptContext is now the running execution context.

        // 11. Let script be scriptRecord.[[ECMAScriptCode]].

        // FIXME: 12. Let result be Completion(GlobalDeclarationInstantiation(script, FIXME: globalEnv)).

        // FIXME: 13. If result.[[Type]] is NORMAL, then
        // a. Set result to Completion(Evaluation of script).
        var result = _statementList.Evaluate(VM);

        // b. If result.[[Type]] is NORMAL and result.[[Value]] is EMPTY, then
        if (result.IsNormalCompletion() && result.IsValueEmpty())
        {
            // i. Set result to NormalCompletion(undefined).
            result = Completion.NormalCompletion(VM.Undefined);
        }

        // FIXME: 14. Suspend scriptContext and remove it from the execution context stack.
        // FIXME: 15. Assert: The execution context stack is not empty.
        // FIXME: 16. Resume the context that is now on the top of the execution context stack as the running execution context.

        // 17. Return ? result.
        return result;
    }

    public VM VM { get; }
    public Realm Realm { get; }
    public IReadOnlyList<INode> ScriptCode
    {
        get { return _statementList.Statements; }
    }
    private readonly StatementList _statementList;
    // FIXME: LoadedModules
}
