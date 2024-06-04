using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.3.3 Properties of the Boolean Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-boolean-prototype-object
// The Boolean prototype object is itself a Boolean object; it has a [[BooleanData]] internal slot with the value false.
internal sealed class BooleanPrototype : BooleanObject 
{
    // The Boolean prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public BooleanPrototype(ObjectPrototype prototype) : base(prototype, false)
    {
    }

    public void Initialize(Realm realm, VM vm)
    {
        // 20.3.3.1 Boolean.prototype.constructor, The initial value of Boolean.prototype.constructor is %Boolean%.
        InternalDefineProperty("constructor", realm.BooleanConstructor, new(true, false, true));

        // 20.3.3.2 Boolean.prototype.toString ( ), https://tc39.es/ecma262/#sec-boolean.prototype.tostring
        InternalDefineProperty(vm, "toString", 0, toString, new(true, false, true));

        // 20.3.3.3 Boolean.prototype.valueOf ( ), https://tc39.es/ecma262/#sec-boolean.prototype.valueof
        InternalDefineProperty(vm, "valueOf", 0, valueOf, new(true, false, true));
    }

    // 20.3.3.2 Boolean.prototype.toString ( ), https://tc39.es/ecma262/#sec-boolean.prototype.tostring
    private Completion toString(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Let b be ? ThisBooleanValue(this value).
        var b = ThisBooleanValue(vm, thisValue);
        if (b.IsAbruptCompletion()) return b.Completion;

        // 2. If b is true, return "true"; else return "false".
        return b.Value ? "true" : "false";
    }

    // 20.3.3.3 Boolean.prototype.valueOf ( ), https://tc39.es/ecma262/#sec-boolean.prototype.valueof
    private Completion valueOf(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Return ? ThisBooleanValue(this value).
        var result = ThisBooleanValue(vm, thisValue);
        if (result.IsAbruptCompletion()) return result.Completion;
        return result.Value;
    }

    // 20.3.3.3.1 ThisBooleanValue ( value ), https://tc39.es/ecma262/#sec-thisbooleanvalue
    private AbruptOr<Boolean> ThisBooleanValue(VM vm, Value value)
    {
        // 1. If value is a Boolean, return value.
        if (value.IsBoolean()) return value.AsBoolean();

        // 2. If value is an Object and value has a [[BooleanData]] internal slot, then
        if (value is BooleanObject obj)
        {
            // a. Let b be value.[[BooleanData]].
            var b = obj.BooleanData;

            // b. Assert: b is a Boolean.

            // c. Return b.
            return b;
        }

        // 3. Throw a TypeError exception.
        return ThrowTypeError(vm, RuntimeErrorType.ThisIsNotABoolean);
    }
}
