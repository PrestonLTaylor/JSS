using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.5.6.1 The NativeError Constructors, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-typeerror
internal class NativeErrorConstructor : Object, ICallable, IConstructable
{
    // Each NativeError constructor has a [[Prototype]] internal slot whose value is %Error%.
    public NativeErrorConstructor(ErrorConstructor prototype) : base(prototype)
    {
    }

    public void Initialize(Object prototype, string name)
    {
        // Each NativeError constructor has a "name" property whose value is the String value "NativeError".
        DataProperties.Add("name", new(name, new(false, false, false)));

        // 20.5.6.2.1 NativeError.prototype, The initial value of NativeError.prototype is a NativeError prototype object (20.5.6.3).
        // Each NativeError constructor has a distinct prototype object.
        DataProperties.Add("prototype", new(prototype, new(false, false, false)));
        _nativePrototype = prototype;
    }

    // 20.5.6.1.1 NativeError ( message [ , options ] ), https://tc39.es/ecma262/#sec-nativeerror
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList, Undefined.The);
    }

    // 20.5.6.1.1 NativeError ( message [ , options ] ), https://tc39.es/ecma262/#sec-nativeerror
    public Completion Construct(VM vm, List argumentsList, Object newTarget)
    {
        // FIXME: 1. If NewTarget is undefined, let newTarget be the active function object; else let newTarget be NewTarget.

        // FIXME: 2. Let O be ? OrdinaryCreateFromConstructor(newTarget, "%NativeError.prototype%", « [[ErrorData]] »).
        var O = new Object(_nativePrototype);

        // 3. If message is not undefined, then
        var message = argumentsList[0];
        if (!message.IsUndefined())
        {
            // a. Let msg be ? ToString(message).
            var msg = message.ToStringJS(vm);
            if (msg.IsAbruptCompletion()) return msg;

            // b. Perform CreateNonEnumerableDataPropertyOrThrow(O, "message", msg).
            CreateNonEnumerableDataPropertyOrThrow(vm, O, "message", msg.Value);
        }

        // 4. Perform ? InstallErrorCause(O, options).
        var options = argumentsList[1];
        var installResult = ErrorConstructor.InstallErrorCause(vm, O, options);
        if (installResult.IsAbruptCompletion()) return installResult;

        // 5. Return O.
        return O;
    }

    private Object? _nativePrototype;
}
