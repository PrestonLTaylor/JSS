using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 23.1.3 Properties of the Array Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-array-prototype-object
internal sealed class ArrayPrototype : Object
{
    // The Array prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public ArrayPrototype(ObjectPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(VM vm)
    {
        // 23.1.3.23 Array.prototype.push ( ...items ), https://tc39.es/ecma262/#sec-array.prototype.push
        var pushBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, push);
        DataProperties.Add("push", new(pushBuiltin, new(true, false, true)));
    }

    // 23.1.3.23 Array.prototype.push ( ...items ), https://tc39.es/ecma262/#sec-array.prototype.push
    public Completion push(VM vm, Value? thisArgument, List argumentList)
    {
        // 1. Let O be ? ToObject(this value).
        var O = thisArgument!.ToObject(vm);

        // 2. Let len be ? LengthOfArrayLike(O).
        var lenResult = O.Value.LengthOfArrayLike();
        if (lenResult.IsAbruptCompletion()) return lenResult.Completion;
        var len = lenResult.Value;

        // 3. Let argCount be the number of elements in items.
        var argCount = argumentList.Count;

        // FIXME: Overflow errors
        // 4. If len + argCount > 2**53 - 1, throw a TypeError exception.
        if (len + argCount > Math.Pow(2, 53) - 1) return ThrowTypeError(vm, RuntimeErrorType.ArrayLengthTooLarge, len + argCount);

        // 5. For each element E of items, do
        foreach (var e in argumentList.Values)
        {
            // FIXME: Don't use C#'s ToString
            // a. Perform ? Set(O, ! ToString(𝔽(len)), E, true).
            var setResult = Set(vm, O.Value, len.ToString(), e, true);
            if (setResult.IsAbruptCompletion()) return setResult;

            // b. Set len to len + 1.
            len += 1;
        }

        // 6. Perform ? Set(O, "length", 𝔽(len), true).
        var lenSetResult = Set(vm, O.Value, "length", len, true);
        if (lenSetResult.IsAbruptCompletion()) return lenSetResult;

        // 7. Return 𝔽(len).
        return len;
    }
}
