using JSS.Lib.Execution;
using JSS.Lib.Runtime;
using System.Diagnostics;

namespace JSS.Lib.AST.Values;

enum ThisMode
{
    STRICT,
    LEXICAL,
    GLOBAL,
}

enum LexicalThisMode
{
    LEXICAL_THIS,
    NON_LEXICAL_THIS,
}

// FIXME: Spec links for FunctionObject when FunctionObject is more fleshed out
internal sealed class FunctionObject : Object, ICallable, IConstructable
{
#pragma warning disable CS8618 // All properties are initialised in OrdinaryFunctionCreate
    private FunctionObject(Object? prototype) : base(prototype)
    {
    }
#pragma warning restore CS8618

    override public bool IsFunction() { return true; }
    override public ValueType Type() { return ValueType.Function; }

    // 10.2.1 [[Call]] ( thisArgument, argumentsList ), https://tc39.es/ecma262/#sec-ecmascript-function-objects-call-thisargument-argumentslist
    public Completion Call(VM vm, Value thisArgument, List argumentsList)
    {
        // 1. Let callerContext be the running execution context.

        // 2. Let calleeContext be PrepareForOrdinaryCall(F, undefined).
        var calleeContext = (PrepareForOrdinaryCall(vm, Undefined.The) as ScriptExecutionContext)!;

        // 3. Assert: calleeContext is now the running execution context.
        Debug.Assert(vm.CurrentExecutionContext == calleeContext);

        // FIXME: 4. If F.[[IsClassConstructor]] is true, then
        // FIXME: a. Let error be a newly created TypeError object.
        // FIXME: b. NOTE: error is created in calleeContext with F's associated Realm Record.
        // FIXME: c. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
        // FIXME: d. Return ThrowCompletion(error).

        // 5. Perform OrdinaryCallBindThis(F, calleeContext, thisArgument).
        OrdinaryCallBindThis(vm, calleeContext, thisArgument);

        // 6. Let result be Completion(OrdinaryCallEvaluateBody(F, argumentsList)).
        var result = OrdinaryCallEvaluateBody(vm, argumentsList);

        // 7. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
        vm.PopExecutionContext();

        // 8. If result.[[Type]] is RETURN, return result.[[Value]].
        if (result.IsReturnCompletion()) return result.Value;

        // 9. ReturnIfAbrupt(result).
        if (result.IsAbruptCompletion()) return result;

        // 10. Return undefined.
        return Undefined.The;
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

    // 10.2.1.2 OrdinaryCallBindThis ( F, calleeContext, thisArgument ), https://tc39.es/ecma262/#sec-ordinarycallbindthis
    private void OrdinaryCallBindThis(VM vm, ScriptExecutionContext calleeContext, Value thisArgument)
    {
        // 1. Let thisMode be F.[[ThisMode]].

        // 2. If thisMode is LEXICAL, return UNUSED.
        if (ThisMode == ThisMode.LEXICAL) return;

        // 3. Let calleeRealm be FIXME: F.[[Realm]].
        var calleeRealm = vm.Realm;

        // 4. Let localEnv be the LexicalEnvironment of calleeContext.
        var localEnv = calleeContext.LexicalEnvironment;

        Value thisValue;

        // 5. If thisMode is STRICT, then
        if (ThisMode == ThisMode.STRICT)
        {
            // a. Let thisValue be thisArgument.
            thisValue = thisArgument;
        }
        // 6. Else,
        else
        {
            // a. If thisArgument is either undefined or null, then
            if (thisArgument.IsUndefined() || thisArgument.IsNull())
            {
                // i. Let globalEnv be calleeRealm.[[GlobalEnv]].
                var globalEnv = calleeRealm.GlobalEnv;

                // ii. Assert: globalEnv is a Global Environment Record.
                Debug.Assert(globalEnv is GlobalEnvironment);

                // iii. Let thisValue be globalEnv.[[GlobalThisValue]].
                thisValue = globalEnv.GlobalThisValue;
            }
            // b. Else,
            else
            {
                // i. Let thisValue be ! ToObject(thisArgument).
                thisValue = MUST(thisArgument.ToObject());

                // ii. NOTE: ToObject produces wrapper objects using calleeRealm.
            }
        }

        // 7. Assert: localEnv is a Function Environment Record.
        Debug.Assert(localEnv is FunctionEnvironment);

        // 8. Assert: The next step never returns an abrupt completion because localEnv.[[ThisBindingStatus]] is not INITIALIZED.

        // 9. Perform ! localEnv.BindThisValue(thisValue).
        var localFunctionEnv = localEnv as FunctionEnvironment;
        MUST(localFunctionEnv!.BindThisValue(thisValue));

        // 10. Return UNUSED.
    }

    // 10.2.1.3 Runtime Semantics: EvaluateBody, https://tc39.es/ecma262/#sec-runtime-semantics-evaluatebody
    private Completion EvaluateBody(VM vm, List argumentsList)
    {
        // FIXME: Evaluate other types of functions when we implement them
        // 1. Return ? EvaluateFunctionBody of FunctionBody with arguments functionObject and argumentsList.
        return EvaluateFunctionBody(vm, argumentsList);
    }

    // 10.2.1.4 OrdinaryCallEvaluateBody ( F, argumentsList ), https://tc39.es/ecma262/#sec-ordinarycallevaluatebody
    private Completion OrdinaryCallEvaluateBody(VM vm, List argumentsList)
    {
        // 1. Return ? EvaluateBody of F.[[ECMAScriptCode]] with arguments F and argumentsList.
        return EvaluateBody(vm, argumentsList);
    }

    // 10.2.2 [[Construct]] ( argumentsList, FIXME: newTarget ), https://tc39.es/ecma262/#sec-ecmascript-function-objects-construct-argumentslist-newtarget
    public Completion Construct(VM vm, List argumentsList)
    {
        // 1. Let callerContext be the running execution context.
        var callerContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;

        // 2. Let kind be F.[[ConstructorKind]].

        Object thisArgument = Undefined.The;

        // 3. If kind is BASE, then
        if (ConstructorKind == ConstructorKind.BASE)
        {
            // a. Let thisArgument be ? FIXME: OrdinaryCreateFromConstructor(newTarget, "%Object.prototype%").
            thisArgument = new Object(ObjectPrototype.The);
        }

        // 4. Let calleeContext be PrepareForOrdinaryCall(F, FIXME: newTarget).
        var calleeContext = (PrepareForOrdinaryCall(vm, Undefined.The) as ScriptExecutionContext)!;

        // 5. Assert: calleeContext is now the running execution context.
        Debug.Assert(vm.CurrentExecutionContext == calleeContext);

        // 6. If kind is BASE, then
        if (ConstructorKind == ConstructorKind.BASE)
        {
            // a. Perform OrdinaryCallBindThis(F, calleeContext, thisArgument).
            OrdinaryCallBindThis(vm, calleeContext, thisArgument);

            // FIXME: b. Let initializeResult be Completion(InitializeInstanceElements(thisArgument, F)).
            // FIXME: c. If initializeResult is an abrupt completion, then
            // FIXME: i. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
            // FIXME: ii. Return ? initializeResult.
        }

        // 7. Let constructorEnv be the LexicalEnvironment of calleeContext.
        var constructorEnv = calleeContext.LexicalEnvironment;

        // 8. Let result be Completion(OrdinaryCallEvaluateBody(F, argumentsList)).
        var result = OrdinaryCallEvaluateBody(vm, argumentsList);

        // 9. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
        vm.PopExecutionContext();

        // 10. If result.[[Type]] is RETURN, then
        if (result.IsReturnCompletion())
        {
            // a. If result.[[Value]] is an Object, return result.[[Value]].
            if (result.Value.IsObject()) return result.Value;

            // b. If kind is BASE, return thisArgument.
            if (ConstructorKind == ConstructorKind.BASE) return thisArgument;

            // c. If result.[[Value]] is not undefined, throw a FIXME: TypeError exception.
            if (!result.Value.IsUndefined()) return Completion.ThrowCompletion("Function constructor without kind of base did not return an object/undefined");
        }
        // 11. Else,
        else
        {
            // a. ReturnIfAbrupt(result).
            if (result.IsAbruptCompletion()) return result;
        }

        // 12. Let thisBinding be ? constructorEnv.GetThisBinding().
        var thisBinding = constructorEnv!.GetThisBinding();
        if (thisBinding.IsAbruptCompletion()) return thisBinding;

        // 13. Assert: thisBinding is an Object.
        Debug.Assert(thisBinding.Value is Object);

        // 14. Return thisBinding.
        return thisBinding;
    }

    // 10.2.3 OrdinaryFunctionCreate ( FIXME: functionPrototype, FIXME: sourceText, ParameterList, Body, thisMode, env, FIXME: privateEnv ), https://tc39.es/ecma262/#sec-ordinaryfunctioncreate
    static public FunctionObject OrdinaryFunctionCreate(Object functionPrototype, IReadOnlyList<Identifier> parameterList, StatementList body, LexicalThisMode thisMode,
        Environment env)
    {
        // 1. Let internalSlotsList be the internal slots listed in Table 30.

        // 2. Let F be OrdinaryObjectCreate(functionPrototype, internalSlotsList).
        var f = new FunctionObject(functionPrototype);

        // 3. Set F.[[Call]] to the definition specified in 10.2.1.

        // FIXME: 4. Set F.[[SourceText]] to sourceText.

        // 5. Set F.[[FormalParameters]] to ParameterList.
        f.FormalParameters = parameterList;

        // 6. Set F.[[ECMAScriptCode]] to Body.
        f.ECMAScriptCode = body;

        // FIXME: 7. If the source text matched by Body is strict mode code, let Strict be true; else let Strict be false.
        // FIXME: 8. Set F.[[Strict]] to Strict.

        // 9. If thisMode is LEXICAL-THIS, set F.[[ThisMode]] to LEXICAL.
        if (thisMode == LexicalThisMode.LEXICAL_THIS)
        {
            f.ThisMode = ThisMode.LEXICAL;
        }
        // FIXME: 10. Else if Strict is true, set F.[[ThisMode]] to STRICT.
        // 11. Else, set F.[[ThisMode]] to GLOBAL.
        else
        {
            f.ThisMode = ThisMode.GLOBAL;
        }

        // FIXME: 12. Set F.[[IsClassConstructor]] to false.

        // 13. Set F.[[Environment]] to env.
        f.Environment = env;

        // FIXME: 14. Set F.[[PrivateEnvironment]] to privateEnv.
        // FIXME: 15. Set F.[[ScriptOrModule]] to GetActiveScriptOrModule().
        // FIXME: 16. Set F.[[Realm]] to the current Realm Record.
        // FIXME: 17. Set F.[[HomeObject]] to undefined.
        // FIXME: 18. Set F.[[Fields]] to a new empty List.
        // FIXME: 19. Set F.[[PrivateMethods]] to a new empty List.
        // FIXME: 20. Set F.[[ClassFieldInitializerName]] to EMPTY.

        // FIXME: 21. Let len be the ExpectedArgumentCount of ParameterList.
        // FIXME: 22. Perform SetFunctionLength(F, len).

        // 23. Return F.
        return f;
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

    // 10.2.5 MakeConstructor ( F [ , writablePrototype [ , prototype ] ] ), https://tc39.es/ecma262/#sec-makeconstructor
    public void MakeConstructor(bool? writiablePrototype = null, Object? prototype = null)
    {
        // NOTE: This is implemented using inheritance so these steps are done at compile time
        // FIXME: 1. If F is an ECMAScript function object, then
        // a. Assert: IsConstructor(F) is false.
        // b. Assert: F is an extensible object that does not have a "prototype" own property.
        // c. Set F.[[Construct]] to the definition specified in 10.2.2.
        // FIXME: 2. Else,
        // FIXME: a. Set F.[[Construct]] to the definition specified in 10.3.2.

        // 3. Set F.[[ConstructorKind]] to BASE.
        ConstructorKind = ConstructorKind.BASE;

        // 4. If writablePrototype is not present, set writablePrototype to true.
        writiablePrototype ??= true;

        // 5. If prototype is not present, then
        if (prototype is null)
        {
            // a. Set prototype to OrdinaryObjectCreate(%Object.prototype%).
            prototype = new Object(ObjectPrototype.The);

            // b. Perform ! DefinePropertyOrThrow(prototype, "constructor", PropertyDescriptor { [[Value]]: F, [[Writable]]: writablePrototype, [[Enumerable]]: false, [[Configurable]]: true }).
            MUST(DefinePropertyOrThrow(prototype, "constructor", new(this, new(writiablePrototype.Value, false, true))));
        }

        // 6. Perform ! DefinePropertyOrThrow(F, "prototype", PropertyDescriptor { [[Value]]: prototype, [[Writable]]: writablePrototype, [[Enumerable]]: false, [[Configurable]]: false }).
        MUST(DefinePropertyOrThrow(this, "prototype", new(prototype, new(writiablePrototype.Value, false, false))));

        // 7. Return UNUSED.
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
        MUST(DefinePropertyOrThrow(this, "name", new Property(name, new Attributes(false, false, true))));

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
                MUST(env.CreateMutableBinding(paramName, false));

                // ii. If hasDuplicates is true, then
                if (hasDuplicates)
                {
                    // 1. Perform ! env.InitializeBinding(paramName, undefined).
                    MUST(env.InitializeBinding(paramName, Undefined.The));
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
            MUST(env.InitializeBinding(FormalParameters[i].Name, argumentsList.Values[i]));
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
                MUST(varEnv.CreateMutableBinding(n, false));

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
                    initialValue = MUST(env.GetBindingValue(n, false));
                }

                // 5. Perform ! varEnv.InitializeBinding(n, initialValue).
                MUST(varEnv.InitializeBinding(n, initialValue));

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
                    MUST(lexEnv.CreateImmutableBinding(dn, true));
                }
                // ii. Else,
                else
                {
                    // 1. Perform ! lexEnv.CreateMutableBinding(dn, false).
                    MUST(lexEnv.CreateMutableBinding(dn, false));
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
            MUST(varEnv.SetMutableBinding(fn, fo, false));
        }

        // 37. Return UNUSED.
        return Empty.The;
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

    public IReadOnlyList<Identifier> FormalParameters { get; private set; }
    public StatementList ECMAScriptCode { get; private set; }
    public Environment Environment { get; private set; }
    public ThisMode ThisMode { get; private set; }
    public ConstructorKind ConstructorKind { get; private set; }
}
