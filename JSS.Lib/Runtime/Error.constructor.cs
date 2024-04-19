using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.5.1 The Error Constructor, https://tc39.es/ecma262/#sec-error-constructor
internal sealed class ErrorConstructor : Object, ICallable, IConstructable
{
    // The Error constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public ErrorConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    // 20.5.2 Properties of the Error Constructor, https://tc39.es/ecma262/#sec-properties-of-the-error-constructor
    public void Initialize(Realm realm)
    {
        // 20.5.2.1 Error.prototype, The initial value of Error.prototype is the Error prototype object.
        // This property has the attributes { [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: false }.
        DataProperties.Add("prototype", new(realm.ErrorPrototype, new(false, false, false)));
    }

    // 20.5.1.1 Error ( message [ , options ] ), https://tc39.es/ecma262/#sec-error-message
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList);
    }

    // 20.5.1.1 Error ( message [ , options ] ), https://tc39.es/ecma262/#sec-error-message
    public Completion Construct(VM vm, List argumentsList)
    {
        // FIXME: 1. If NewTarget is undefined, let newTarget be the active function object; else let newTarget be NewTarget.

        // FIXME: 2. Let O be ? OrdinaryCreateFromConstructor(newTarget, "%Error.prototype%", « [[ErrorData]] »).
        var O = new Object(vm.ErrorPrototype);

        // 3. If message is not undefined, then
        var message = argumentsList[0];
        if (!message.IsUndefined())
        {
            // a. Let msg be ? ToString(message).
            var msg = message.ToStringJS();
            if (msg.IsAbruptCompletion()) return msg;

            // b. Perform CreateNonEnumerableDataPropertyOrThrow(O, "message", msg).
            CreateNonEnumerableDataPropertyOrThrow(O, "message", msg.Value);
        }

        // 4. Perform ? InstallErrorCause(O, options).
        var options = argumentsList[1];
        InstallErrorCause(O, options);

        // 5. Return O.
        return O;
    }

    // 20.5.8.1 InstallErrorCause ( O, options ), https://tc39.es/ecma262/#sec-installerrorcause
    static public Completion InstallErrorCause(Object O, Value options)
    {
        // 1. If options is an Object and ? HasProperty(options, "cause") is true, then
        if (options.IsObject())
        {
            var obj = options.AsObject();
            var hasProperty = HasProperty(obj, "cause");
            if (hasProperty.IsAbruptCompletion()) return hasProperty;

            if (hasProperty.Value.AsBoolean())
            {
                // a. Let cause be ? Get(options, "cause").
                var cause = Get(obj, "cause");
                if (cause.IsAbruptCompletion()) return cause;

                // b. Perform CreateNonEnumerableDataPropertyOrThrow(O, "cause", cause).
                CreateNonEnumerableDataPropertyOrThrow(O, "cause", cause.Value);
            }
        }

        // 2. Return UNUSED.
        return Empty.The;
    }
}
