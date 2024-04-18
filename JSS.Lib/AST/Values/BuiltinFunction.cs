using JSS.Lib.Execution;
using JSS.Lib.Runtime;

namespace JSS.Lib.AST.Values;

// 10.3 Built-in Function Objects, https://tc39.es/ecma262/#sec-built-in-function-objects
internal sealed class BuiltinFunction : Object, ICallable, IConstructable
{
#pragma warning disable CS8618 // All properties are initialised in OrdinaryFunctionCreate
    private BuiltinFunction(Object prototype, Func<Value?, List, Completion> behaviour) : base(prototype)
    {
        Behaviour = behaviour;
    }
#pragma warning restore CS8618

    // 10.3.1 [[Call]] ( thisArgument, argumentsList ), https://tc39.es/ecma262/#sec-built-in-function-objects-call-thisargument-argumentslist
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        // 1. Return ? BuiltinCallOrConstruct(F, thisArgument, argumentsList, undefined).
        return BuiltinCallOrConstruct(vm, thisArgument, argumentList);
    }

    // 10.3.2 [[Construct]] ( argumentsList, FIXME: newTarget ), https://tc39.es/ecma262/#sec-built-in-function-objects-construct-argumentslist-newtarget
    public Completion Construct(VM vm, List argumentsList)
    {
        // 1. Return ? BuiltinCallOrConstruct(F, UNINITIALIZED, argumentsList, newTarget).
        return BuiltinCallOrConstruct(vm, null, argumentsList);
    }

    // 10.3.3 BuiltinCallOrConstruct ( F, thisArgument, argumentsList, FIXME: newTarget ), https://tc39.es/ecma262/#sec-builtincallorconstruct
    private Completion BuiltinCallOrConstruct(VM vm, Value? thisArgument, List argumentsList)
    {
        // 1. Let callerContext be the running execution context.
        var callerContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;

        // FIXME: 2. If callerContext is not already suspended, suspend callerContext.

        // 3. Let calleeContext be a new execution context.
        // FIXME: 4. Set the Function of calleeContext to F.
        // 5. Let calleeRealm be F.[[Realm]].
        // 6. Set the Realm of calleeContext to calleeRealm.
        // 7. Set the ScriptOrModule of calleeContext to null.
        var calleeContext = new ScriptExecutionContext(Realm);

        // 8. Perform any necessary implementation-defined initialization of calleeContext.

        // 9. Push calleeContext onto the execution context stack; calleeContext is now the running execution context.
        vm.PushExecutionContext(calleeContext);

        // 10. Let result be the Completion Record that is the result of evaluating F in a manner that conforms to the specification of F.
        // If thisArgument is UNINITIALIZED, the this value is uninitialized; otherwise, thisArgument provides the this value.
        // argumentsList provides the named parameters. newTarget provides the NewTarget value.
        var result = Behaviour(thisArgument, argumentsList);

        // 11. NOTE: If F is defined in this document, “the specification of F” is the behaviour specified for it via algorithm steps or other means.

        // 12. Remove calleeContext from the execution context stack and restore callerContext as the running execution context.
        vm.PopExecutionContext();

        // 13. Return ? result.
        return result;
    }

    // 10.3.4 CreateBuiltinFunction ( behaviour, FIXME: length, FIXME: name, FIXME: additionalInternalSlotsList [ , realm [ , prototype [ , FIXME: prefix ] ] ] ), https://tc39.es/ecma262/#sec-createbuiltinfunction
    static public BuiltinFunction CreateBuiltinFunction(VM vm, Func<Value?, List, Completion> behaviour, Realm? realm = null, Object? prototype = null)
    {
        // 1. If realm is not present, set realm to the current Realm Record.
        realm ??= vm.Realm;

        // 2. If prototype is not present, set prototype to realm.[[Intrinsics]].[[%Function.prototype%]].
        prototype ??= vm.FunctionPrototype;

        // 3. Let internalSlotsList be a List containing the names of all the internal slots that 10.3 requires for the built-in function object that is about to be created.

        // 4. Append to internalSlotsList the elements of additionalInternalSlotsList.

        // 5. Let func be a new built-in function object that, when called,
        // performs the action described by behaviour using the provided arguments as the values of the corresponding parameters specified by behaviour.
        // The new function object has internal slots whose names are the elements of internalSlotsList, and an [[InitialName]] internal slot.
        // 6. Set func.[[Prototype]] to prototype.
        var func = new BuiltinFunction(prototype, behaviour);

        // FIXME: 7. Set func.[[Extensible]] to true.

        // 8. Set func.[[Realm]] to realm.
        func.Realm = realm;

        // FIXME: 9. Set func.[[InitialName]] to null.

        // FIXME: 10. Perform SetFunctionLength(func, length).

        // FIXME: 11. If prefix is not present, then
        // FIXME: a. Perform SetFunctionName(func, name).
        // FIXME: 12. Else,
        // FIXME: a. Perform SetFunctionName(func, name, prefix).

        // 13. Return func.
        return func;
    }

    public Func<Value?, List, Completion> Behaviour { get; private set; }
    public Realm Realm { get; private set; }
}
