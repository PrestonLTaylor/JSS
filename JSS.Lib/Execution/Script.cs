using JSS.Lib.AST;

namespace JSS.Lib.Execution;

// 16.1.4 Script Records, https://tc39.es/ecma262/#sec-script-records 
internal sealed class Script
{
    public Script(List<INode> rootNodes)
    {
        VM = new();
        Realm = new();
        ScriptCode = rootNodes;
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
        var result = Evaluate();

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

    // 14.2.2 Runtime Semantics: Evaluation, StatementList, https://tc39.es/ecma262/#sec-block-runtime-semantics-evaluation
    private Completion Evaluate()
    {
        // FIXME: This is not quite how it's laid out in the spec
        // 1. Let sl be ? Evaluation of StatementList.
        Completion completion = Completion.NormalCompletion();

        foreach (var node in ScriptCode)
        {
            // 2. Let s be Completion(Evaluation of StatementListItem).
            var s = node.Evaluate(VM);

            // 3. Return ? UpdateEmpty(s, sl).
            s.UpdateEmpty(completion.Value);
            completion = s;

            // FIXME: Maybe find a more eliquant way to do ReturnIfAbrupt
            if (completion.IsAbruptCompletion())
            {
                return completion;
            }
        }

        return completion;
    }

    public VM VM { get; }
    public Realm Realm { get; }
    // FIXME: We should probably have a StatementList class
    public IReadOnlyList<INode> ScriptCode { get; }
    // FIXME: LoadedModules
}
