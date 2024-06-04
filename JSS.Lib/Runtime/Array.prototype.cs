using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Text;

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
        // 23.1.3.18 Array.prototype.join ( separator ), https://tc39.es/ecma262/#sec-array.prototype.join
        InternalDefineProperty(vm, "join", 1, join, new(true, false, true));

        // 23.1.3.23 Array.prototype.push ( ...items ), https://tc39.es/ecma262/#sec-array.prototype.push
        InternalDefineProperty(vm, "push", 1, push, new(true, false, true));
    }

    // 23.1.3.18 Array.prototype.join ( separator ), https://tc39.es/ecma262/#sec-array.prototype.join
    private Completion join(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Let O be ? ToObject(this value).
        var O = thisArgument.ToObject(vm);
        if (O.IsAbruptCompletion()) return O.Completion;

        // 2. Let len be ? LengthOfArrayLike(O).
        var lenResult = O.Value.LengthOfArrayLike(vm);
        if (lenResult.IsAbruptCompletion()) return lenResult;
        var len = (int)lenResult.Value;

        // 3. If separator is undefined, let sep be ",".
        string sep;
        if (argumentList[0].IsUndefined())
        {
            sep = ",";
        }
        // 4. Else, let sep be ? ToString(separator).
        else
        {
            var toString = argumentList[0].ToStringJS(vm);
            if (toString.IsAbruptCompletion()) return toString.Completion;
            sep = toString.Value;
        }

        // 5. Let R be the empty String.
        StringBuilder R = new();

        // 6. Let k be 0.
        var k = 0;

        // 7. Repeat, while k < len,
        while (k < len)
        {
            // a. If k > 0, set R to the string-concatenation of R and sep.
            if (k > 0)
            {
                R.Append(sep);
            }

            // b. Let element be ? Get(O, ! ToString(𝔽(k))).
            var element = Get(O.Value, k.ToString());
            if (element.IsAbruptCompletion()) return element;

            // c. If element is neither undefined nor null, then
            if (!element.Value.IsUndefined() && !element.Value.IsNull())
            {
                // i. Let S be ? ToString(element).
                var S = element.Value.ToStringJS(vm);
                if (S.IsAbruptCompletion()) return S.Completion;

                // ii. Set R to the string-concatenation of R and S.
                R.Append(S.Value);
            }

            // d. Set k to k + 1.
            k += 1;
        }

        // 8. Return R.
        return R.ToString();
    }

    // 23.1.3.23 Array.prototype.push ( ...items ), https://tc39.es/ecma262/#sec-array.prototype.push
    private Completion push(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Let O be ? ToObject(this value).
        var O = thisArgument.ToObject(vm);

        // 2. Let len be ? LengthOfArrayLike(O).
        var lenResult = O.Value.LengthOfArrayLike(vm);
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
