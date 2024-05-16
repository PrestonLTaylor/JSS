using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

internal sealed class ErrorPrototype : Object
{
    // The Error prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public ErrorPrototype(ObjectPrototype prototype) : base(prototype)
    {
    }

    // 20.5.3 Properties of the Error Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-error-prototype-object
    public void Initialize(Realm realm, VM vm)
    {
        // 20.5.3.1 Error.prototype.constructor, The initial value of Error.prototype.constructor is %Error%.
        DataProperties.Add("constructor", new Property(realm.ErrorConstructor, new(true, false, false)));

        // 20.5.3.2 Error.prototype.message, The initial value of Error.prototype.message is the empty String.
        DataProperties.Add("message", new Property("", new(true, false, false)));

        // 20.5.3.3 Error.prototype.name, The initial value of Error.prototype.name is "Error".
        DataProperties.Add("name", new Property("Error", new(true, false, false)));

        // 20.5.3.4 Error.prototype.toString ( ), https://tc39.es/ecma262/#sec-error.prototype.tostring
        var toStringBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, toString, 0, "toString");
        DataProperties.Add("toString", new Property(toStringBuiltin, new(false, false, false)));
    }

    // 20.5.3.4 Error.prototype.toString ( ), https://tc39.es/ecma262/#sec-error.prototype.tostring
    private Completion toString(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Let O be the this value.

        // 2. If O is not an Object, throw a TypeError exception.
        if (!thisArgument.IsObject()) return ThrowTypeError(vm, RuntimeErrorType.ThisIsNotAnObject);

        // 3. Let name be ? Get(O, "name").
        var O = thisArgument.AsObject();
        var getName = Get(O, "name");
        if (getName.IsAbruptCompletion()) return getName;

        // 4. If name is undefined, set name to "Error"; otherwise set name to ? ToString(name).
        string name;
        if (getName.Value.IsUndefined())
        {
            name = "Error";
        }
        else
        {
            var toString = getName.Value.ToStringJS(vm);
            if (toString.IsAbruptCompletion()) return toString.Completion;
            name = toString.Value;
        }

        // 5. Let msg be ? Get(O, "message").
        var getMsg = Get(O, "message");
        if (getMsg.IsAbruptCompletion()) return getMsg;

        // 6. If msg is undefined, set msg to the empty String; otherwise set msg to ? ToString(msg).
        string msg;
        if (getName.Value.IsUndefined())
        {
            msg = "";
        }
        else
        {
            var toString = getMsg.Value.ToStringJS(vm);
            if (toString.IsAbruptCompletion()) return toString.Completion;
            msg = toString.Value;
        }

        // 7. If name is the empty String, return msg.
        if (name == "") return msg;

        // 8. If msg is the empty String, return name.
        if (msg == "") return name;

        // 9. Return the string-concatenation of name, the code unit 0x003A (COLON), the code unit 0x0020 (SPACE), and msg.
        return name + ": " + msg;
    }
}
