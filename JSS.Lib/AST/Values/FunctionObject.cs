using JSS.Lib.Execution;
using System.Diagnostics;
using Environment = JSS.Lib.Execution.Environment;
using ExecutionContext = JSS.Lib.Execution.ExecutionContext;

namespace JSS.Lib.AST.Values;

// FIXME: Spec links for FunctionObject when FunctionObject is more fleshed out
internal sealed class FunctionObject : Object, ICallable
{
    public FunctionObject(IReadOnlyList<Identifier> formalParameters, StatementList body, Environment env) : base(null)
    {
        FormalParameters = formalParameters;
        ECMAScriptCode = body;
        Environment = env;
    }

    override public bool IsFunction() { return true; }
    override public ValueType Type() { return ValueType.Function; }

    // 10.2.1 [[Call]] ( thisArgument, argumentsList ), https://tc39.es/ecma262/#sec-ecmascript-function-objects-call-thisargument-argumentslist
    public Completion Call(VM vm, Value thisArgument, List argumentsList)
    {
        // 1. Let callerContext be the running execution context.

        // 2. Let calleeContext be PrepareForOrdinaryCall(F, undefined).
        var calleeContext = PrepareForOrdinaryCall(vm, Undefined.The);

        // 3. Assert: calleeContext is now the running execution context.
        Debug.Assert(vm.CurrentExecutionContext == calleeContext);

        // FIXME: 4. If F.[[IsClassConstructor]] is true, then
        // FIXME: a. Let error be a newly created TypeError object.
        // FIXME: b. NOTE: error is created in calleeContext with F's associated Realm Record.
        // FIXME: c. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
        // FIXME: d. Return ThrowCompletion(error).
        // FIXME: 5. Perform OrdinaryCallBindThis(F, calleeContext, thisArgument).

        // 6. Let result be Completion(OrdinaryCallEvaluateBody(F, argumentsList)).
        var result = OrdinaryCallEvaluateBody(vm, argumentsList);

        // 7. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
        vm.PopExecutionContext();

        // 8. If result.[[Type]] is RETURN, return result.[[Value]].
        if (result.IsReturnCompletion()) return Completion.NormalCompletion(result.Value);

        // 9. ReturnIfAbrupt(result).
        if (result.IsAbruptCompletion()) return result;

        // 10. Return undefined.
        return Completion.NormalCompletion(Undefined.The);
    }

    // 10.2.1.1 PrepareForOrdinaryCall ( F, newTarget ), https://tc39.es/ecma262/#sec-prepareforordinarycall
    private ExecutionContext PrepareForOrdinaryCall(VM vm, Object newTarget)
    {
        // 1. Let callerContext be the running execution context.
        var callerContext = vm.CurrentExecutionContext;

        // 2. Let calleeContext be a new ECMAScript code execution context.
        // FIXME: 3. Set the Function of calleeContext to F.
        // 4. Let calleeRealm be F.[[Realm]].
        // 5. Set the Realm of calleeContext to calleeRealm.
        // 6. Set the ScriptOrModule of calleeContext to F.[[ScriptOrModule]].
        var calleeContext = new ScriptExecutionContext(vm.Realm);

        // 7. Let localEnv be NewFunctionEnvironment(F, newTarget).
        var localEnv = new FunctionEnvironment(this, newTarget);

        // 8. Set the LexicalEnvironment of calleeContext to localEnv.
        calleeContext.LexicalEnvironment = localEnv;

        // 9. Set the VariableEnvironment of calleeContext to localEnv.
        calleeContext.VariableEnvironment = localEnv;

        // FIXME: 10. Set the PrivateEnvironment of calleeContext to F.[[PrivateEnvironment]].

        // FIXME: 11. If callerContext is not already suspended, suspend callerContext.

        // 12. Push calleeContext onto the execution context stack; calleeContext is now the running execution context.
        vm.PushExecutionContext(calleeContext);

        // 13. NOTE: Any exception objects produced after this point are associated with calleeRealm.
        // 14. Return calleeContext.
        return calleeContext;
    }

    // 10.2.1.3 Runtime Semantics: EvaluateBody, https://tc39.es/ecma262/#sec-runtime-semantics-evaluatebody
    private Completion EvaluateBody(VM vm, List argumentsList)
    {
        // FIXME: Evaluate other types of functions when we implement them
        // 1. Return ? EvaluateFunctionBody of FunctionBody with arguments functionObject and argumentsList.
        return EvaluateFunctionBody(vm, argumentsList);
    }

    // FIXME: This should be here
    // 15.2.3 Runtime Semantics: EvaluateFunctionBody, https://tc39.es/ecma262/#sec-runtime-semantics-evaluatefunctionbody
    private Completion EvaluateFunctionBody(VM vm, List argumentsList)
    {
        // 1. Perform ? FunctionDeclarationInstantiation(functionObject, argumentsList).
        var instantiationResult = FunctionDeclarationInstantiation(vm, argumentsList);
        if (instantiationResult.IsAbruptCompletion()) return instantiationResult;

        // 2. Return ? Evaluation of FunctionStatementList.
        return ECMAScriptCode.Evaluate(vm);
    }

    // 10.2.1.4 OrdinaryCallEvaluateBody ( F, argumentsList ), https://tc39.es/ecma262/#sec-ordinarycallevaluatebody
    private Completion OrdinaryCallEvaluateBody(VM vm, List argumentsList)
    {
        // 1. Return ? EvaluateBody of F.[[ECMAScriptCode]] with arguments F and argumentsList.
        return EvaluateBody(vm, argumentsList);
    }

    // 10.2.9 SetFunctionName ( F, name [ , prefix ] ), https://tc39.es/ecma262/#sec-setfunctionname
    public void SetFunctionName(string name, string prefix = "")
    {
        // 1. Assert: FIXME: (F is an extensible object) that does not have a "name" own property.
        Debug.Assert(!DataProperties.ContainsKey("name"));

        // FIXME: 2. If name is a Symbol, then
        // FIXME: a. Let description be name's [[Description]] value.
        // FIXME: b. If description is undefined, set name to the empty String.
        // FIXME: c. Else, set name to the string-concatenation of "[", description, and "]".
        // FIXME: 3. Else if name is a Private Name, then
        // FIXME: a. Set name to name.[[Description]].
        // FIXME: 4. If F has an [[InitialName]] internal slot, then
        // FIXME: a. Set F.[[InitialName]] to name.

        // 5. If prefix is present, then
        if (!string.IsNullOrEmpty(prefix))
        {
            // a. Set name to the string-concatenation of prefix, the code unit 0x0020 (SPACE), and name.
            name = prefix + " " + name;

            // FIXME: b. If F has an [[InitialName]] internal slot, then
            // FIXME: i. Optionally, set F.[[InitialName]] to name.
        }

        // 6. Perform ! DefinePropertyOrThrow(F, "name", PropertyDescriptor { [[Value]]: name, [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: true }).
        var result = DefinePropertyOrThrow(this, "name", new Property(new String(name), new Attributes(false, false, true)));
        Debug.Assert(result.IsNormalCompletion());

        // 7. Return unused.
    }

    // 10.2.11 FunctionDeclarationInstantiation ( func, argumentsList ), https://tc39.es/ecma262/#sec-functiondeclarationinstantiation
    private Completion FunctionDeclarationInstantiation(VM vm, List argumentsList)
    {
        // 1. Let calleeContext be the running execution context.
        var calleeContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;

        // 2. Let code be func.[[ECMAScriptCode]].
        // FIXME: 3. Let strict be func.[[Strict]].
        // 4. Let formals be func.[[FormalParameters]].

        // 5. Let parameterNames be the BoundNames of formals.
        var parameterNames = FormalParameterBoundNames();

        // 6. If parameterNames has any duplicate entries, let hasDuplicates be true. Otherwise, let hasDuplicates be false.
        var hasDuplicates = parameterNames.Count != parameterNames.Distinct().Count();

        // FIXME: 7. Let simpleParameterList be IsSimpleParameterList of formals.
        // FIXME: 8. Let hasParameterExpressions be ContainsExpression of formals.

        // 9. Let varNames be the VarDeclaredNames of code.
        var varNames = ECMAScriptCode.VarDeclaredNames();

        // 10. Let varDeclarations be the VarScopedDeclarations of code.
        var varDeclarations = ECMAScriptCode.VarScopedDeclarations();

        // 11. Let lexicalNames be the LexicallyDeclaredNames of code.
        var lexicalNames = ECMAScriptCode.LexicallyDeclaredNames();

        // 12. Let functionNames be a new empty List.
        List<string> functionNames = new();

        // 13. Let functionsToInitialize be a new empty List.
        List<FunctionDeclaration> functionsToInitialize = new();

        // 14. For each element d of varDeclarations, in reverse List order, do
        for (int i = varDeclarations.Count - 1; i >= 0; --i)
        {
            var d = varDeclarations[i];

            // a. If d is neither a VariableDeclaration nor a ForBinding nor a BindingIdentifier, then
            if (d is not Identifier)
            {
                // i. Assert: d is either a FunctionDeclaration, a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration.
                Debug.Assert(d is FunctionDeclaration);

                // ii. Let fn be the sole element of the BoundNames of d.
                var fn = d.BoundNames().FirstOrDefault()!;

                // iii. If functionNames does not contain fn, then
                if (!functionNames.Contains(fn))
                {
                    // 1. Insert fn as the first element of functionNames.
                    functionNames.Insert(0, fn);

                    // 2. NOTE: If there are multiple function declarations for the same name, the last declaration is used.

                    // 3. Insert d as the first element of functionsToInitialize.
                    functionsToInitialize.Insert(0, (d as FunctionDeclaration)!);
                }
            }
        }

        // 15. Let argumentsObjectNeeded be true.
        var argumentsObjectNeeded = true;

        // FIXME: 16. If func.[[ThisMode]] is LEXICAL, then
        // FIXME: a. NOTE: Arrow functions never have an arguments object.
        // FIXME: b. Set argumentsObjectNeeded to false.

        // 17. Else if parameterNames contains "arguments", then
        if (parameterNames.Contains("arguments"))
        {
            // a. Set argumentsObjectNeeded to false.
            argumentsObjectNeeded = false;
        }
        // FIXME: 18. Else if hasParameterExpressions is false, then
        // FIXME: a. If functionNames contains "arguments" or lexicalNames contains "arguments", then
        // FIXME: i. Set argumentsObjectNeeded to false.

        // FIXME: 19. If strict is true or hasParameterExpressions is false, then
        // FIXME: a. NOTE: Only a single Environment Record is needed for the parameters, since calls to eval in strict mode code cannot create new bindings
        // which are visible outside of the eval.
        // FIXME: b. Let env be the LexicalEnvironment of calleeContext.
        // FIXME: 20. Else,
        // a. NOTE: A separate Environment Record is needed to ensure that bindings created by direct eval calls in the formal parameter list are outside the
        // environment where parameters are declared.

        // b. Let calleeEnv be the LexicalEnvironment of calleeContext.
        var calleeEnv = calleeContext.LexicalEnvironment;

        // c. Let env be NewDeclarativeEnvironment(calleeEnv).
        var env = new DeclarativeEnvironment(calleeEnv);

        // d. Assert: The VariableEnvironment of calleeContext is calleeEnv.
        Debug.Assert(calleeContext.VariableEnvironment == calleeEnv);

        // e. Set the LexicalEnvironment of calleeContext to env.
        calleeContext.LexicalEnvironment = env;

        // 21. For each String paramName of parameterNames, do
        foreach (var paramName in parameterNames)
        {
            // a. Let alreadyDeclared be ! env.HasBinding(paramName).
            var alreadyDeclared = env.HasBinding(paramName);

            // FIXME: b. NOTE: Early errors ensure that duplicate parameter names can only occur in non-strict functions that do not have parameter default values or rest parameters.

            // c. If alreadyDeclared is false, then
            if (!alreadyDeclared)
            {
                // i. Perform ! env.CreateMutableBinding(paramName, false).
                var createResult = env.CreateMutableBinding(paramName, false);
                Debug.Assert(createResult.IsNormalCompletion());

                // ii. If hasDuplicates is true, then
                if (hasDuplicates)
                {
                    // 1. Perform ! env.InitializeBinding(paramName, undefined).
                    var initializeResult = env.InitializeBinding(paramName, Undefined.The);
                    Debug.Assert(initializeResult.IsNormalCompletion());
                }
            }
        }

        // FIXME: 22. If argumentsObjectNeeded is true, then
        // FIXME: a. If strict is true or simpleParameterList is false, then
        // FIXME: i. Let ao be CreateUnmappedArgumentsObject(argumentsList).
        // FIXME: b. Else,
        // FIXME: i. NOTE: A mapped argument object is only provided for non - strict functions that don't have a rest parameter, any parameter default value initializers, or any destructured parameters.
        // FIXME: ii. Let ao be CreateMappedArgumentsObject(func, formals, argumentsList, env).
        // FIXME: c. If strict is true, then
        // FIXME: i. Perform ! env.CreateImmutableBinding("arguments", false).
        // FIXME: ii. NOTE: In strict mode code early errors prevent attempting to assign to this binding, so its mutability is not observable.
        // FIXME: d. Else,
        // FIXME: i. Perform ! env.CreateMutableBinding("arguments", false).
        // FIXME: e. Perform ! env.InitializeBinding("arguments", ao).
        // FIXME: f. Let parameterBindings be the list - concatenation of parameterNames and « "arguments" ».
        // FIXME: 23. Else,
        // a. Let parameterBindings be parameterNames.
        var parameterBindings = parameterNames;

        // FIXME: 24. Let iteratorRecord be CreateListIteratorRecord(argumentsList).
        // FIXME: 25. If hasDuplicates is true, then
        // FIXME: a. Perform ? IteratorBindingInitialization of formals with arguments iteratorRecord and undefined.
        // FIXME: 26. Else,
        // FIXME: a. Perform ? IteratorBindingInitialization of formals with arguments iteratorRecord and env.
        for (int i = 0; i < Math.Min(FormalParameters.Count, argumentsList.Values.Count); ++i)
        {
            var initResult = env.InitializeBinding(FormalParameters[i].Name, argumentsList.Values[i]);
            Debug.Assert(initResult.IsNormalCompletion());
        }

        // FIXME: 27. If hasParameterExpressions is false, then
        // FIXME: a. NOTE: Only a single Environment Record is needed for the parameters and top-level vars.
        // FIXME: b. Let instantiatedVarNames be a copy of the List parameterBindings.
        // FIXME: c. For each element n of varNames, do
        // FIXME: i. If instantiatedVarNames does not contain n, then
        // FIXME: 1. Append n to instantiatedVarNames.
        // FIXME: 2. Perform ! env.CreateMutableBinding(n, false).
        // FIXME: 3. Perform ! env.InitializeBinding(n, undefined).
        // FIXME: d. Let varEnv be env.
        // FIXME: 28. Else,

        // a. NOTE: A separate Environment Record is needed to ensure that closures created by expressions in the formal parameter list do not have visibility of declarations in the function body.
        // b. Let varEnv be NewDeclarativeEnvironment(env).
        var varEnv = new DeclarativeEnvironment(env);

        // c. Set the VariableEnvironment of calleeContext to varEnv.
        calleeContext.VariableEnvironment = varEnv;

        // d. Let instantiatedVarNames be a new empty List.
        List<string> instantiatedVarNames = new();

        // e. For each element n of varNames, do
        foreach (var n in varNames)
        {
            // i. If instantiatedVarNames does not contain n, then
            if (!instantiatedVarNames.Contains(n))
            {
                // 1. Append n to instantiatedVarNames.
                instantiatedVarNames.Add(n);

                // 2. Perform ! varEnv.CreateMutableBinding(n, false).
                var createResult = varEnv.CreateMutableBinding(n, false);
                Debug.Assert(createResult.IsNormalCompletion());

                // 3. If parameterBindings does not contain n, or if functionNames contains n, then
                Value initialValue;
                if (!parameterBindings.Contains(n) || functionNames.Contains(n))
                {
                    // a. Let initialValue be undefined.
                    initialValue = Undefined.The;
                }
                // 4. Else,
                else
                {
                    // a. Let initialValue be ! env.GetBindingValue(n, false).
                    var getResult = env.GetBindingValue(n, false);
                    Debug.Assert(getResult.IsNormalCompletion());
                    initialValue = getResult.Value;
                }

                // 5. Perform ! varEnv.InitializeBinding(n, initialValue).
                var initializeResult = varEnv.InitializeBinding(n, initialValue);
                Debug.Assert(initializeResult.IsNormalCompletion());

                // 6. NOTE: A var with the same name as a formal parameter initially has the same value as the corresponding initialized parameter.
            }
        }

        // 29. NOTE: Annex B.3.2.1 adds additional steps at this point.

        // FIXME: 30. If strict is false, then
        // FIXME: a. Let lexEnv be NewDeclarativeEnvironment(varEnv).
        // FIXME: b. NOTE: Non-strict functions use a separate Environment Record for top-level lexical declarations so that a direct eval can determine whether
        // any var scoped declarations introduced by the eval code conflict with pre-existing top-level lexically scoped declarations. This is not needed for
        // strict functions because a strict direct eval always places all declarations into a new Environment Record.
        // FIXME: 31. Else,
        // a. Let lexEnv be varEnv.
        var lexEnv = varEnv;

        // 32. Set the LexicalEnvironment of calleeContext to lexEnv.
        calleeContext.LexicalEnvironment = lexEnv;

        // 33. Let lexDeclarations be the LexicallyScopedDeclarations of code.
        var lexDeclarations = ECMAScriptCode.LexicallyScopedDeclarations();

        // 34. For each element d of lexDeclarations, do
        foreach (var d in lexDeclarations)
        {
            // a. NOTE: A lexically declared name cannot be the same as a function/generator declaration, formal parameter, or a var name.
            // Lexically declared names are only instantiated here but not initialized.

            // b. For each element dn of the BoundNames of d, do
            foreach (var dn in d.BoundNames())
            {
                // i. If IsConstantDeclaration of d is true, then
                if (d is ConstDeclaration)
                {
                    // 1. Perform ! lexEnv.CreateImmutableBinding(dn, true).
                    var createResult = lexEnv.CreateImmutableBinding(dn, true);
                    Debug.Assert(createResult.IsNormalCompletion());
                }
                // ii. Else,
                else
                {
                    // 1. Perform ! lexEnv.CreateMutableBinding(dn, false).
                    var createResult = lexEnv.CreateMutableBinding(dn, false);
                    Debug.Assert(createResult.IsNormalCompletion());
                }
            }
        }

        // FIXME: 35. Let privateEnv be the PrivateEnvironment of calleeContext.

        // 36. For each Parse Node f of functionsToInitialize, do
        foreach (var f in functionsToInitialize)
        {
            // a. Let fn be the sole element of the BoundNames of f.
            var fn = f.BoundNames().FirstOrDefault()!;

            // b. Let fo be InstantiateFunctionObject of f with arguments lexEnv and privateEnv.
            var fo = f.InstantiateFunctionObject(lexEnv);

            // c. Perform ! varEnv.SetMutableBinding(fn, fo, false).
            var setResult = varEnv.SetMutableBinding(fn, fo, false);
            Debug.Assert(setResult.IsNormalCompletion());
        }

        // 37. Return UNUSED.
        return Completion.NormalCompletion(Empty.The);
    }

    // FIXME: We should have a Parameters parse node that we can call BoundNames on
    private List<string> FormalParameterBoundNames()
    {
        // 1. Let names1 be BoundNames of FormalParameterList.
        List<string> names = new();
        foreach (var formal in FormalParameters)
        {
            // 2. Let names2 be BoundNames of FunctionRestParameter.
            names.AddRange(formal.BoundNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    public IReadOnlyList<Identifier> FormalParameters { get; }
    public StatementList ECMAScriptCode { get; }
    public Environment Environment { get; }
}
