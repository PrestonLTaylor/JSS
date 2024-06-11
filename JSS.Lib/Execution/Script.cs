using JSS.Lib.AST.Values;
using JSS.Lib.AST;

namespace JSS.Lib.Execution;

// 16.1.4 Script Records, https://tc39.es/ecma262/#sec-script-records 
public sealed class Script
{
    internal Script(VM vm, StatementList statementList, bool isStrict)
    {
        VM = vm;
        Realm = vm.Realm;
        Body = statementList;
        IsStrict = isStrict;
    }

    // 16.1.6 ScriptEvaluation ( scriptRecord ), https://tc39.es/ecma262/#sec-runtime-semantics-scriptevaluation
    public Completion ScriptEvaluation(CancellationToken cancellationToken = default)
    {
        VM.CancellationToken = cancellationToken;

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
        VM.PushStrictness(IsStrict);

        // 11. Let script be scriptRecord.[[ECMAScriptCode]].

        // 12. Let result be Completion(GlobalDeclarationInstantiation(script, globalEnv)).
        var result = GlobalDeclarationInstantiation(globalEnv!);

        // 13. If result.[[Type]] is NORMAL, then
        if (result.IsNormalCompletion())
        {
            // a. Set result to Completion(Evaluation of script).
            result = Body.Evaluate(VM);
        }

        // b. If result.[[Type]] is NORMAL and result.[[Value]] is EMPTY, then
        if (result.IsNormalCompletion() && result.IsValueEmpty())
        {
            // i. Set result to NormalCompletion(undefined).
            result = Undefined.The;
        }

        // 14. (FIXME: Suspend) scriptContext and remove it from the execution context stack.
        VM.PopExecutionContext();
        VM.PopStrictness();

        // 15. Assert: The execution context stack is not empty.
        Assert(VM.HasExecutionContext(), "15. Assert: The execution context stack is not empty.");

        // FIXME: 16. Resume the context that is now on the top of the execution context stack as the running execution context.

        // 17. Return ? result.
        return result;
    }

    // 16.1.7 GlobalDeclarationInstantiation ( script, env ), https://tc39.es/ecma262/#sec-globaldeclarationinstantiation
    private Completion GlobalDeclarationInstantiation(GlobalEnvironment env)
    {
        // 1. Let lexNames be the LexicallyDeclaredNames of script.
        var lexNames = LexicallyDeclaredNames();

        // 2. Let varNames be the VarDeclaredNames of script.
        var varNames = VarDeclaredNames();

        // 3. For each element name of lexNames, do
        foreach (var name in lexNames)
        {
            // a. If env.HasVarDeclaration(name) is true, throw a SyntaxError exception.
            if (env.HasVarDeclaration(name)) return ThrowSyntaxError(VM, RuntimeErrorType.RedeclarationOfVar, name);

            // b. If env.HasLexicalDeclaration(name) is true, throw a SyntaxError exception.
            if (env.HasLexicalDeclaration(name)) return ThrowSyntaxError(VM, RuntimeErrorType.RedeclarationOfLet, name);

            // c. Let hasRestrictedGlobal be ? env.HasRestrictedGlobalProperty(name).
            var hasRestrictedGlobal = env.HasRestrictedGlobalProperty(name);
            if (hasRestrictedGlobal.IsAbruptCompletion()) return hasRestrictedGlobal;

            // d. If hasRestrictedGlobal is true, throw a SyntaxError exception.
            var asBoolean = hasRestrictedGlobal.Value.AsBoolean();
            if (asBoolean.Value) ThrowSyntaxError(VM, RuntimeErrorType.RedeclarationOfUnconfigurable, name);
        }

        // 4. For each element name of varNames, do
        foreach (var name in varNames)
        {
            // a. If env.HasLexicalDeclaration(name) is true, throw a SyntaxError exception.
            if (env.HasLexicalDeclaration(name)) ThrowSyntaxError(VM, RuntimeErrorType.RedeclarationOfLet, name);
        }

        // 5. Let varDeclarations be the VarScopedDeclarations of script.
        var varDeclarations = VarScopedDeclarations();

        // 6. Let functionsToInitialize be a new empty List.
        List<FunctionDeclaration> functionsToInitialize = new();

        // 7. Let declaredFunctionNames be a new empty List.
        List<string> declaredFunctionNames = new();

        // 8. For each element d of varDeclarations, in reverse List order, do
        for (var i = varDeclarations.Count - 1; i >= 0; --i)
        {
            var d = varDeclarations[i];

            // a. If d is not either a VariableDeclaration, a ForBinding, or a BindingIdentifier, then
            if (d is not VarDeclaration and not Identifier)
            {
                // i. Assert: d is either a FunctionDeclaration, FIXME: a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration.
                Assert(d is FunctionDeclaration, "i. Assert: d is either a FunctionDeclaration, FIXME: a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration.");

                // ii. NOTE: If there are multiple function declarations for the same name, the last declaration is used.

                // iii. Let fn be the sole element of the BoundNames of d.
                var fn = d.BoundNames().FirstOrDefault()!;

                // iv. If declaredFunctionNames does not contain fn, then
                if (!declaredFunctionNames.Contains(fn))
                {
                    // 1. Let fnDefinable be ? env.CanDeclareGlobalFunction(fn).
                    var fnDefinable = env.CanDeclareGlobalFunction(fn);
                    if (fnDefinable.IsAbruptCompletion()) return fnDefinable;

                    // 2. If fnDefinable is false, throw a TypeError exception.
                    var asBoolean = fnDefinable.Value.AsBoolean();
                    if (!asBoolean.Value) return ThrowTypeError(VM, RuntimeErrorType.RedeclarationOfUnconfigurableFunction, fn);

                    // 3. Append fn to declaredFunctionNames.
                    declaredFunctionNames.Add(fn);

                    // 4. Insert d as the first element of functionsToInitialize.
                    functionsToInitialize.Insert(0, (d as FunctionDeclaration)!);
                }
            }
        }

        // 9. Let declaredVarNames be a new empty List.
        List<string> declaredVarNames = new();

        // 10. For each element d of varDeclarations, do
        foreach (var d in varDeclarations)
        {
            // a. If d is either a VariableDeclaration, a ForBinding, or a BindingIdentifier, then
            if (d is VarDeclaration or Identifier)
            {
                // i. For each String vn of the BoundNames of d, do
                foreach (var vn in d.BoundNames())
                {
                    // 1. If declaredFunctionNames does not contain vn, then
                    if (!declaredFunctionNames.Contains(vn))
                    {
                        // a. Let vnDefinable be ? env.CanDeclareGlobalVar(vn).
                        var vnDefinable = env.CanDeclareGlobalVar(vn);
                        if (vnDefinable.IsAbruptCompletion()) return vnDefinable;

                        // b. If vnDefinable is false, throw a TypeError exception.
                        var asBoolean = vnDefinable.Value.AsBoolean();
                        if (!asBoolean.Value) return ThrowTypeError(VM, RuntimeErrorType.RedeclarationOfUnextensibleVar, vn);

                        // c. If declaredVarNames does not contain vn, then
                        if (!declaredVarNames.Contains(vn))
                        {
                            // i. Append vn to declaredVarNames.
                            declaredVarNames.Add(vn);
                        }
                    }
                }
            }
        }

        // 11. NOTE: No abnormal terminations occur after this algorithm step if the global object is an ordinary object.However, if the global object is a Proxy exotic object it may exhibit behaviours that cause abnormal terminations in some of the following steps.
        // 12. NOTE: Annex B.3.2.2 adds additional steps at this point.

        // 13. Let lexDeclarations be the LexicallyScopedDeclarations of script.
        List<INode> lexDeclarations = LexicallyScopedDeclarations();

        // FIXME: 14. Let privateEnv be null.

        // 15. For each element d of lexDeclarations, do
        foreach (var d in lexDeclarations)
        {
            // a. NOTE: Lexically declared names are only instantiated here but not initialized.

            // b. For each element dn of the BoundNames of d, do
            foreach (var dn in d.BoundNames())
            {
                // i. If IsConstantDeclaration of d is true, then
                if (d is ConstDeclaration)
                {
                    // 1. Perform ? env.CreateImmutableBinding(dn, true).
                    var createResult = env.CreateImmutableBinding(VM, dn, true);
                    if (createResult.IsAbruptCompletion()) return createResult;
                }
                // ii. Else,
                else
                {
                    // 1. Perform ? env.CreateMutableBinding(dn, false).
                    var createResult = env.CreateMutableBinding(VM, dn, false);
                    if (createResult.IsAbruptCompletion()) return createResult;
                }
            }
        }

        // 16. For each Parse Node f of functionsToInitialize, do
        foreach (var f in functionsToInitialize)
        {
            // a. Let fn be the sole element of the BoundNames of f.
            var fn = f.BoundNames().FirstOrDefault()!;

            // b. Let fo be InstantiateFunctionObject of f with arguments env FIXME: and privateEnv.
            var fo = f.InstantiateFunctionObject(VM, env);

            // c. Perform ? env.CreateGlobalFunctionBinding(fn, fo, false).
            var createResult = env.CreateGlobalFunctionBinding(VM, fn, fo, false);
            if (createResult.IsAbruptCompletion()) return createResult;
        }

        // 17. For each String vn of declaredVarNames, do
        foreach (var vn in declaredVarNames)
        {
            // a. Perform ? env.CreateGlobalVarBinding(vn, false).
            var createResult = env.CreateGlobalVarBinding(VM, vn, false);
            if (createResult.IsAbruptCompletion()) return createResult;
        }

        // 18. Return UNUSUED.
        return Empty.The;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    private List<string> LexicallyDeclaredNames()
    {
        // 1. Return TopLevelLexicallyDeclaredNames of StatementList.
        return Body.TopLevelLexicallyDeclaredNames();
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    private List<INode> LexicallyScopedDeclarations()
    {
        // 1. Return TopLevelLexicallyScopedDeclarations of StatementList.
        return Body.TopLevelLexicallyScopedDeclarations();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    private List<string> VarDeclaredNames()
    {
        // 1. Return TopLevelVarDeclaredNames of StatementList.
        return Body.TopLevelVarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    private List<INode> VarScopedDeclarations()
    {
        // 1. Return TopLevelVarScopedDeclarations of StatementList.
        return Body.TopLevelVarScopedDeclarations();
    }

    public VM VM { get; }
    public Realm Realm { get; }
    internal IReadOnlyList<INode> ScriptCode
    {
        get { return Body.Statements; }
    }
    internal StatementList Body { get; }
    internal bool IsStrict { get; }
    // FIXME: LoadedModules
}
