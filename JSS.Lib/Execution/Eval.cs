using JSS.Lib.AST;
using JSS.Lib.AST.Values;

namespace JSS.Lib.Execution;

internal static class Eval
{
    // 19.2.1 eval ( x ), https://tc39.es/ecma262/#sec-eval-x
    static public Completion eval(VM vm, Value thisValue, List argumentList)
    {
        // 1. Return ? PerformEval(x, false, false).
        return PerformEval(vm, argumentList[0], false, false);
    }

    // 19.2.1.1 PerformEval ( x, strictCaller, direct ), https://tc39.es/ecma262/#sec-performeval
    static public Completion PerformEval(VM vm, Value x, bool strictCaller, bool direct)
    {
        // 1. Assert: If direct is false, then strictCaller is also false.
        if (!direct) Assert(!strictCaller, "1. Assert: If direct is false, then strictCaller is also false.");

        // 2. If x is not a String, return x.
        if (!x.IsString()) return x;
        var xStr = x.AsString();

        // 3. Let evalRealm be the current Realm Record.
        var evalRealm = vm.Realm;

        // 4. NOTE: In the case of a direct eval, evalRealm is the realm of both the caller of eval and of the eval function itself.

        // 5. Perform ? HostEnsureCanCompileStrings(evalRealm, « », x, direct).
        var canCompile = vm.HostEnsureCanCompileStrings(evalRealm, new(), xStr, direct);
        if (canCompile.IsAbruptCompletion()) return canCompile;

        // 6. Let inFunction be false.
        var inFunction = false;

        // 7. Let inMethod be false.
        var inMethod = false;

        // 8. Let inDerivedConstructor be false.
        var inDerivedConstructor = false;

        // 9. Let inClassFieldInitializer be false.
        var inClassFieldInitializer = false;

        // 10. If direct is true, then
        if (direct)
        {
            // a. Let thisEnvRec be GetThisEnvironment().
            var thisEnvRec = ScriptExecutionContext.GetThisEnvironment(vm);

            // b. If thisEnvRec is a Function Environment Record, then
            if (thisEnvRec is FunctionEnvironment)
            {
                // i. Let F be thisEnvRec.[[FunctionObject]].
                var funcEnvRec = thisEnvRec as FunctionEnvironment;
                var F = funcEnvRec!.FunctionObject;

                // ii. Set inFunction to true.
                inFunction = true;

                // FIXME: iii. Set inMethod to thisEnvRec.HasSuperBinding().

                // iv. If F.[[ConstructorKind]] is DERIVED, set inDerivedConstructor to true.
                if (F.ConstructorKind == ConstructorKind.DERIVED)
                {
                    inDerivedConstructor = true;
                }

                // FIXME: v. Let classFieldInitializerName be F.[[ClassFieldInitializerName]].
                // FIXME: vi. If classFieldInitializerName is not EMPTY, set inClassFieldInitializer to true.
            }
        }

        // 11. Perform the following substeps in an implementation-defined order, possibly interleaving parsing and error detection:
        // a. Let script be FIXME: ParseText(x, Script).
        var parser = new Parser(xStr);

        Script script;
        try
        {
            script = parser.Parse(vm);
        }
        catch (SyntaxErrorException ex)
        {
            // b. If script is a List of errors, throw a SyntaxError exception.
            return ThrowSyntaxError(vm, RuntimeErrorType.EvalParsingFailed, ex.Message);
        }

        // FIXME: c. If script Contains ScriptBody is false, return undefined.

        // d. Let body be the ScriptBody of script.
        var body = script.Body;

        // FIXME: e. If inFunction is false and body Contains NewTarget, throw a SyntaxError exception.
        // FIXME: f. If inMethod is false and body Contains SuperProperty, throw a SyntaxError exception.
        // FIXME: g. If inDerivedConstructor is false and body Contains SuperCall, throw a SyntaxError exception.
        // FIXME: h. If inClassFieldInitializer is true and ContainsArguments of body is true, throw a SyntaxError exception.

        // 12. If strictCaller is true, let strictEval be true.
        // 13. Else, let strictEval be ScriptIsStrict of script.
        var strictEval = strictCaller || script.IsStrict;

        // 14. Let runningContext be the running execution context.
        var runningContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;

        // 15. NOTE: If direct is true, runningContext will be the execution context that performed the direct eval.
        // If direct is false, runningContext will be the execution context for the invocation of the eval function.

        // 16. If direct is true, then
        DeclarativeEnvironment lexEnv;
        Environment varEnv;
        if (direct)
        {
            // a. Let lexEnv be NewDeclarativeEnvironment(runningContext's LexicalEnvironment).
            lexEnv = new DeclarativeEnvironment(runningContext.LexicalEnvironment);

            // b. Let varEnv be runningContext's VariableEnvironment.
            varEnv = runningContext.VariableEnvironment!;

            // FIXME: c. Let privateEnv be runningContext's PrivateEnvironment.
        }
        // 17. Else,
        else
        {
            // a. Let lexEnv be NewDeclarativeEnvironment(evalRealm.[[GlobalEnv]]).
            lexEnv = new DeclarativeEnvironment(evalRealm.GlobalEnv);

            // b. Let varEnv be evalRealm.[[GlobalEnv]].
            varEnv = evalRealm.GlobalEnv!;

            // FIXME: c. Let privateEnv be null.
        }

        // 18. If strictEval is true, set varEnv to lexEnv.
        if (strictEval) varEnv = lexEnv;

        // FIXME: 19. If runningContext is not already suspended, suspend runningContext.

        // 20. Let evalContext be a new ECMAScript code execution context.
        // FIXME: 21. Set evalContext's Function to null.
        // 22. Set evalContext's Realm to evalRealm.
        // FIXME: 23. Set evalContext's ScriptOrModule to runningContext's ScriptOrModule.
        var evalContext = new ScriptExecutionContext(evalRealm);

        // 24. Set evalContext's VariableEnvironment to varEnv.
        evalContext.VariableEnvironment = varEnv;

        // 25. Set evalContext's LexicalEnvironment to lexEnv.
        evalContext.LexicalEnvironment = lexEnv;

        // FIXME: 26. Set evalContext's PrivateEnvironment to privateEnv.

        // 27. Push evalContext onto the execution context stack; evalContext is now the running execution context.
        vm.PushExecutionContext(evalContext);
        vm.PushStrictness(strictEval);

        // 28. Let result be Completion(EvalDeclarationInstantiation(body, varEnv, lexEnv, FIXME: privateEnv, strictEval)).
        var result = EvalDeclarationInstantiation(vm, body, varEnv, lexEnv, strictEval);

        // 29. If result is a normal completion, then
        if (result.IsNormalCompletion())
        {
            // a. Set result to Completion(Evaluation of body).
            result = body.Evaluate(vm);
        }
        // 30. If result is a normal completion and result.[[Value]] is EMPTY, then
        if (result.IsNormalCompletion() && result.IsValueEmpty())
        {
            // a. Set result to NormalCompletion(undefined).
            result = Undefined.The;
        }

        // 31. FIXME (Suspend evalContext) and remove it from the execution context stack.
        vm.PopExecutionContext();
        vm.PopStrictness();

        // FIXME: 32. Resume the context that is now on the top of the execution context stack as the running execution context.

        // 33. Return ? result.
        return result;
    }

    // 19.2.1.3 EvalDeclarationInstantiation ( body, varEnv, lexEnv, privateEnv, strict ), https://tc39.es/ecma262/#sec-evaldeclarationinstantiation
    static private Completion EvalDeclarationInstantiation(VM vm, StatementList body, Environment varEnv, DeclarativeEnvironment lexEnv, bool strict)
    {
        // 1. Let varNames be the VarDeclaredNames of body.
        var varNames = body.VarDeclaredNames();

        // 2. Let varDeclarations be the VarScopedDeclarations of body.
        var varDeclarations = body.VarScopedDeclarations();

        // 3. If strict is false, then
        if (!strict)
        {
            // a. If varEnv is a Global Environment Record, then
            if (varEnv is GlobalEnvironment)
            {
                var globalVarEnv = varEnv as GlobalEnvironment;

                // i. For each element name of varNames, do
                foreach (var name in varNames)
                {
                    // 1. If varEnv.HasLexicalDeclaration(name) is true, throw a SyntaxError exception.
                    if (globalVarEnv!.HasLexicalDeclaration(name)) return ThrowSyntaxError(vm, RuntimeErrorType.RedeclarationOfVar, name);

                    // 2. NOTE: eval will not create a global var declaration that would be shadowed by a global lexical declaration.
                }

                // b. Let thisEnv be lexEnv.
                Environment thisEnv = lexEnv;

                // c. Assert: The following loop will terminate.

                // d. Repeat, while thisEnv and varEnv are not the same Environment Record,
                while (thisEnv != varEnv)
                {
                    // i. If thisEnv is not an Object Environment Record, then
                    if (thisEnv is not ObjectEnvironment)
                    {
                        // 1. NOTE: The environment of with statements cannot contain any lexical declaration so it doesn't need to be checked for var/let hoisting conflicts.

                        // 2. For each element name of varNames, do
                        foreach (var name in varNames)
                        {
                            // a. If ! thisEnv.HasBinding(name) is true, then
                            if (thisEnv.HasBinding(name))
                            {
                                // i. Throw a SyntaxError exception.
                                return ThrowSyntaxError(vm, RuntimeErrorType.RedeclarationOfVar, name);

                                // ii. NOTE: Annex B.3.4 defines alternate semantics for the above step.
                            }

                            // b. NOTE: A direct eval will not hoist var declaration over a like-named lexical declaration.
                        }
                    }

                    // ii. Set thisEnv to thisEnv.[[OuterEnv]].
                    thisEnv = thisEnv.OuterEnv!;
                }
            }
        }

        // FIXME: 4. Let privateIdentifiers be a new empty List.
        // FIXME: 5. Let pointer be privateEnv.
        // FIXME: 6. Repeat, while pointer is not null,
        // FIXME: a. For each Private Name binding of pointer.[[Names]], do
        // FIXME: i. If privateIdentifiers does not contain binding.[[Description]], append binding.[[Description]] to privateIdentifiers.
        // FIXME: b. Set pointer to pointer.[[OuterPrivateEnvironment]].
        // FIXME: 7. If AllPrivateIdentifiersValid of body with argument privateIdentifiers is false, throw a SyntaxError exception.

        // 8. Let functionsToInitialize be a new empty List.
        List<FunctionDeclaration> functionsToInitialize = new();

        // 9. Let declaredFunctionNames be a new empty List.
        List<string> declaredFunctionNames = new();

        // 10. For each element d of varDeclarations, in reverse List order, do
        for (int i = varDeclarations.Count - 1; i >= 0; --i)
        {
            var d = varDeclarations[i];

            // a. If d is not either a VariableDeclaration, a ForBinding, or a BindingIdentifier, then
            if (d is not VarDeclaration and not Identifier)
            {
                // i. Assert: d is either a FunctionDeclaration, FIXME: a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration.
                Assert(d is FunctionDeclaration, "i. Assert: d is either a FunctionDeclaration, FIXME: a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration.");

                // ii. NOTE: If there are multiple function declarations for the same name, the last declaration is used.

                // iii. Let fn be the sole element of the BoundNames of d.
                var fn = d.BoundNames()[0];

                // iv. If declaredFunctionNames does not contain fn, then
                if (!declaredFunctionNames.Contains(fn))
                {
                    // 1. If varEnv is a Global Environment Record, then
                    if (varEnv is GlobalEnvironment)
                    {
                        var globalVarEnv = varEnv as GlobalEnvironment;

                        // a. Let fnDefinable be ? varEnv.CanDeclareGlobalFunction(fn).
                        var fnDefinable = globalVarEnv!.CanDeclareGlobalFunction(fn);
                        if (fnDefinable.IsAbruptCompletion()) return fnDefinable;

                        // b. If fnDefinable is false, throw a TypeError exception.
                        var asBoolean = fnDefinable.Value.AsBoolean();
                        if (!asBoolean) return ThrowTypeError(vm, RuntimeErrorType.RedeclarationOfUnconfigurableFunction, fn);
                    }

                    // 2. Append fn to declaredFunctionNames.
                    declaredFunctionNames.Add(fn);

                    // 3. Insert d as the first element of functionsToInitialize.
                    functionsToInitialize.Insert(0, (d as FunctionDeclaration)!);
                }
            }
        }

        // 11. Let declaredVarNames be a new empty List.
        List<string> declaredVarNames = new();

        // 12. For each element d of varDeclarations, do
        foreach (var d in varDeclarations)
        {
            // a. If d is either a VariableDeclaration, a ForBinding, or a BindingIdentifier, then
            if (d is VarDeclaration or Identifier)
            {
                // i. For each String vn of the BoundNames of d, do
                var boundNames = d.BoundNames();
                foreach (var vn in boundNames)
                {
                    // 1. If declaredFunctionNames does not contain vn, then
                    if (!declaredFunctionNames.Contains(vn))
                    {
                        // a. If varEnv is a Global Environment Record, then
                        if (varEnv is GlobalEnvironment)
                        {
                            var globalVarEnv = varEnv as GlobalEnvironment;

                            // i. Let vnDefinable be ? varEnv.CanDeclareGlobalVar(vn).
                            var vnDefinable = globalVarEnv!.CanDeclareGlobalVar(vn);
                            if (vnDefinable.IsAbruptCompletion()) return vnDefinable;

                            // ii. If vnDefinable is false, throw a TypeError exception.
                            var asBoolean = vnDefinable.Value.AsBoolean();
                            if (!asBoolean.Value) return ThrowTypeError(vm, RuntimeErrorType.RedeclarationOfUnextensibleVar, vn);
                        }

                        // b. If declaredVarNames does not contain vn, then
                        if (!declaredVarNames.Contains(vn))
                        {
                            // i. Append vn to declaredVarNames.
                            declaredVarNames.Add(vn);
                        }
                    }
                }
            }
        }

        // 13. NOTE: Annex B.3.2.3 adds additional steps at this point.

        // 14. NOTE: No abnormal terminations occur after this algorithm step unless varEnv is a Global Environment Record and the global object is a Proxy exotic object.

        // 15. Let lexDeclarations be the LexicallyScopedDeclarations of body.
        var lexDeclarations = body.LexicallyScopedDeclarations();

        // 16. For each element d of lexDeclarations, do
        foreach (var d in lexDeclarations)
        {
            // a. NOTE: Lexically declared names are only instantiated here but not initialized.

            // b. For each element dn of the BoundNames of d, do
            var boundNames = d.BoundNames();
            foreach (var dn in boundNames)
            {
                // i. If IsConstantDeclaration of d is true, then
                if (d is ConstDeclaration)
                {
                    // 1. Perform ? lexEnv.CreateImmutableBinding(dn, true).
                    var createResult = lexEnv.CreateImmutableBinding(vm, dn, true);
                    if (createResult.IsAbruptCompletion()) return createResult;
                }
                // ii. Else,
                else
                {
                    // 1. Perform ? lexEnv.CreateMutableBinding(dn, false).
                    var createResult = lexEnv.CreateMutableBinding(vm, dn, true);
                    if (createResult.IsAbruptCompletion()) return createResult;
                }
            }
        }

        // 17. For each Parse Node f of functionsToInitialize, do
        foreach (var f in functionsToInitialize)
        {
            // a. Let fn be the sole element of the BoundNames of f.
            var fn = f.BoundNames()[0];

            // b. Let fo be InstantiateFunctionObject of f with arguments lexEnv and privateEnv.
            var fo = f.InstantiateFunctionObject(vm, lexEnv);

            // c. If varEnv is a Global Environment Record, then
            if (varEnv is GlobalEnvironment)
            {
                var globalVarEnv = varEnv as GlobalEnvironment;

                // i. Perform ? varEnv.CreateGlobalFunctionBinding(fn, fo, true).
                var createResult = globalVarEnv!.CreateGlobalFunctionBinding(vm, fn, fo, true);
                if (createResult.IsAbruptCompletion()) return createResult;
            }
            // d. Else,
            else
            {
                // i. Let bindingExists be ! varEnv.HasBinding(fn).
                var bindingExists = varEnv.HasBinding(fn);

                // ii. If bindingExists is false, then
                if (bindingExists)
                {
                    // 1. NOTE: The following invocation cannot return an abrupt completion because of the validation preceding step 14.

                    // 2. Perform ! varEnv.CreateMutableBinding(fn, true).
                    MUST(varEnv.CreateMutableBinding(vm, fn, true));

                    // 3. Perform ! varEnv.InitializeBinding(fn, fo).
                    MUST(varEnv.InitializeBinding(vm, fn, fo));
                }
                // iii. Else,
                else
                {
                    // 1. Perform ! varEnv.SetMutableBinding(fn, fo, false).
                    MUST(varEnv.SetMutableBinding(vm, fn, fo, false));
                }
            }
        }

        // 18. For each String vn of declaredVarNames, do
        foreach (var vn in declaredVarNames)
        {
            // a. If varEnv is a Global Environment Record, then
            if (varEnv is GlobalEnvironment)
            {
                var globalVarEnv = varEnv as GlobalEnvironment;

                // i. Perform ? varEnv.CreateGlobalVarBinding(vn, true).
                var createResult = globalVarEnv!.CreateGlobalVarBinding(vm, vn, true);
                if (createResult.IsAbruptCompletion()) return createResult;
            }
            // b. Else,
            else
            {
                // i. Let bindingExists be ! varEnv.HasBinding(vn).
                var bindingExists = varEnv.HasBinding(vn);

                // ii. If bindingExists is false, then
                if (!bindingExists)
                {
                    // 1. NOTE: The following invocation cannot return an abrupt completion because of the validation preceding step 14.

                    // 2. Perform ! varEnv.CreateMutableBinding(vn, true).
                    MUST(varEnv.CreateMutableBinding(vm, vn, true));

                    // 3. Perform ! varEnv.InitializeBinding(vn, undefined).
                    MUST(varEnv.InitializeBinding(vm, vn, Undefined.The));
                }
            }
        }

        // 19. Return UNUSED.
        return Empty.The;
    }
}
