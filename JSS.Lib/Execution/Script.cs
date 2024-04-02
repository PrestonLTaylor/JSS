﻿using Boolean = JSS.Lib.AST.Values.Boolean;
using JSS.Lib.AST.Values;
using JSS.Lib.AST;
using String = JSS.Lib.AST.Values.String;
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

        // 12. Let result be Completion(GlobalDeclarationInstantiation(script, globalEnv)).
        var result = GlobalDeclarationInstantiation(globalEnv!);

        // 13. If result.[[Type]] is NORMAL, then
        if (result.IsNormalCompletion())
        {
            // a. Set result to Completion(Evaluation of script).
            result = _statementList.Evaluate(VM);
        }

        // b. If result.[[Type]] is NORMAL and result.[[Value]] is EMPTY, then
        if (result.IsNormalCompletion() && result.IsValueEmpty())
        {
            // i. Set result to NormalCompletion(undefined).
            result = Undefined.The;
        }

        // 14. (FIXME: Suspend) scriptContext and remove it from the execution context stack.
        VM.PopExecutionContext();

        // 15. Assert: The execution context stack is not empty.
        Debug.Assert(VM.HasExecutionContext());

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
            // FIXME: Throw SyntaxError Objects
            // a. If env.HasVarDeclaration(name) is true, throw a SyntaxError exception.
            if (env.HasVarDeclaration(name)) return Completion.ThrowCompletion(new String($"redeclaration of var {name}"));

            // b. If env.HasLexicalDeclaration(name) is true, throw a SyntaxError exception.
            if (env.HasLexicalDeclaration(name)) return Completion.ThrowCompletion(new String($"redeclaration of let {name}"));

            // c. Let hasRestrictedGlobal be ? env.HasRestrictedGlobalProperty(name).
            var hasRestrictedGlobal = env.HasRestrictedGlobalProperty(name);
            if (hasRestrictedGlobal.IsAbruptCompletion()) return hasRestrictedGlobal;

            // d. If hasRestrictedGlobal is true, throw a SyntaxError exception.
            var asBoolean = hasRestrictedGlobal.Value.AsBoolean();
            if (asBoolean.Value) return Completion.ThrowCompletion(new String($"redeclaration of Unconfigurable {name}"));
        }

        // 4. For each element name of varNames, do
        foreach (var name in varNames)
        {
            // a. If env.HasLexicalDeclaration(name) is true, throw a SyntaxError exception.
            if (env.HasLexicalDeclaration(name)) return Completion.ThrowCompletion(new String($"redeclaration of let {name}"));
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
            if (d is not VarStatement or Identifier)
            {
                // i. Assert: d is either a FunctionDeclaration, FIXME: a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration.
                Debug.Assert(d is FunctionDeclaration);

                // ii. NOTE: If there are multiple function declarations for the same name, the last declaration is used.

                // iii. Let fn be the sole element of the BoundNames of d.
                var fn = d.BoundNames().FirstOrDefault()!;

                // iv. If declaredFunctionNames does not contain fn, then
                if (!declaredFunctionNames.Contains(fn))
                {
                    // 1. Let fnDefinable be ? env.CanDeclareGlobalFunction(fn).
                    var fnDefinable = env.CanDeclareGlobalFunction(fn);
                    if (fnDefinable.IsAbruptCompletion()) return fnDefinable;

                    // FIXME: Throw an actual TypeError Error
                    // 2. If fnDefinable is false, throw a TypeError exception.
                    var asBoolean = fnDefinable.Value.AsBoolean();
                    if (!asBoolean.Value) return Completion.ThrowCompletion(new String($"redeclaration of Unconfigurable function {fn}"));

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
            if (d is VarStatement or Identifier)
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

                        // b. If vnDefinable is false, FIXME: throw a TypeError exception.
                        var asBoolean = vnDefinable.Value.AsBoolean();
                        if (!asBoolean.Value) return Completion.ThrowCompletion(new String($"redeclaration of Unextensible var {vn}"));

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
                    var createResult = env.CreateImmutableBinding(dn, true);
                    if (createResult.IsAbruptCompletion()) return createResult;
                }
                // ii. Else,
                else
                {
                    // 1. Perform ? env.CreateMutableBinding(dn, false).
                    var createResult = env.CreateMutableBinding(dn, false);
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
            var fo = f.InstantiateFunctionObject(env);

            // c. Perform ? env.CreateGlobalFunctionBinding(fn, fo, false).
            var createResult = env.CreateGlobalFunctionBinding(fn, fo, false);
            if (createResult.IsAbruptCompletion()) return createResult;
        }

        // 17. For each String vn of declaredVarNames, do
        foreach (var vn in declaredVarNames)
        {
            // a. Perform ? env.CreateGlobalVarBinding(vn, false).
            var createResult = env.CreateGlobalVarBinding(vn, false);
            if (createResult.IsAbruptCompletion()) return createResult;
        }

        // 18. Return UNUSUED.
        return Empty.The;
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
