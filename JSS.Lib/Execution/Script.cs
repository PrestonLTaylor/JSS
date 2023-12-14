using JSS.Lib.AST;
using System.Diagnostics;

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
        // 1. Let globalEnv be scriptRecord.[[Realm]].[[GlobalEnv]].
        var globalEnv = Realm.GlobalEnv;

        // 2. Let scriptContext be a new ECMAScript code execution context.
        // FIXME: 3. Set the Function of scriptContext to null.
        // 4. Set the Realm of scriptContext to scriptRecord.[[Realm]].
        var scriptContext = new ScriptExecutionContext(Realm);

        // FIXME: 5. Set the ScriptOrModule of scriptContext to scriptRecord.

        // 6. Set the VariableEnvironment of scriptContext to globalEnv.
        scriptContext.VariableEnvironment = globalEnv;

        // 7. Set the LexicalEnvironment of scriptContext to globalEnv.
        scriptContext.LexicalEnvironment = globalEnv;

        // 8. Set the PrivateEnvironment of scriptContext to null.
        scriptContext.PrivateEnvironment = null;

        // FIXME: 9. Suspend the running execution context.

        // 10. Push scriptContext onto the execution context stack; scriptContext is now the running execution context.
        VM.PushExecutionContext(scriptContext);

        // 11. Let script be scriptRecord.[[ECMAScriptCode]].

        // FIXME: 12. Let result be Completion(GlobalDeclarationInstantiation(script, globalEnv)).

        // FIXME: 13. If result.[[Type]] is NORMAL, then
        // a. Set result to Completion(Evaluation of script).
        var result = _statementList.Evaluate(VM);

        // b. If result.[[Type]] is NORMAL and result.[[Value]] is EMPTY, then
        if (result.IsNormalCompletion() && result.IsValueEmpty())
        {
            // i. Set result to NormalCompletion(undefined).
            result = Completion.NormalCompletion(VM.Undefined);
        }

        // 14. (FIXME: Suspend) scriptContext and remove it from the execution context stack.
        VM.PopExecutionContext();

        // 15. Assert: The execution context stack is not empty.
        Debug.Assert(VM.HasExecutionContext());

        // FIXME: 16. Resume the context that is now on the top of the execution context stack as the running execution context.

        // 17. Return ? result.
        return result;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    private List<string> LexicallyDeclaredNames()
    {
        // 1. Return TopLevelLexicallyDeclaredNames of StatementList.
        return _statementList.TopLevelLexicallyDeclaredNames();
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    private List<INode> LexicallyScopedDeclarations()
    {
        // 1. Return TopLevelLexicallyScopedDeclarations of StatementList.
        return _statementList.TopLevelLexicallyScopedDeclarations();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    private List<string> VarDeclaredNames()
    {
        // 1. Return TopLevelVarDeclaredNames of StatementList.
        return _statementList.TopLevelVarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    private List<INode> VarScopedDeclarations()
    {
        // 1. Return TopLevelVarScopedDeclarations of StatementList.
        return _statementList.TopLevelVarScopedDeclarations();
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
