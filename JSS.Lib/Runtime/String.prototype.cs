using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 22.1.3 Properties of the String Prototype Object, https://tc39.es/ecma262/multipage/text-processing.html#sec-properties-of-the-string-prototype-object
// FIXME: The String prototype object is a String exotic object and has the internal methods specified for such objects.
internal sealed class StringPrototype : Object
{
    // The String prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public StringPrototype(ObjectPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm, VM vm)
    {
        // 22.1.3.1 String.prototype.at ( index ), https://tc39.es/ecma262/#sec-string.prototype.at
        InternalDefineProperty(vm, "at", 1, at, new(true, false, true));

        // 22.1.3.6 String.prototype.constructor, The initial value of String.prototype.constructor is %String%.
        InternalDefineProperty("constructor", realm.StringConstructor, new(true, false, true));

        // 22.1.3.28 String.prototype.toLowerCase ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tolowercase
        InternalDefineProperty(vm, "toLowerCase", 0, toLowerCase, new(true, false, true));

        // 22.1.3.29 String.prototype.toString ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tostring
        InternalDefineProperty(vm, "toString", 0, toString, new(true, false, true));

        // 22.1.3.35 String.prototype.valueOf ( ), https://tc39.es/ecma262/#sec-string.prototype.valueof
        InternalDefineProperty(vm, "valueOf", 0, valueOf, new(true, false, true));
    }

    // 22.1.3.1 String.prototype.at ( index ), https://tc39.es/ecma262/#sec-string.prototype.at
    private Completion at(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Let O be ? RequireObjectCoercible(this value).
        var O = thisValue.RequireObjectCoercible(vm);
        if (O.IsAbruptCompletion()) return O;

        // 2. Let S be ? ToString(O).
        var S = O.Value.ToStringJS(vm);
        if (S.IsAbruptCompletion()) return S.Completion;

        // 3. Let len be the length of S.
        var len = S.Value.Length;

        // 4. Let relativeIndex be ? ToIntegerOrInfinity(index).
        var relativeIndex = argumentList[0].ToIntegerOrInfinity(vm);
        if (relativeIndex.IsAbruptCompletion()) return relativeIndex.Completion;

        // 5. If relativeIndex ≥ 0, then
        int k;
        if (relativeIndex.Value >= 0)
        {
            k = (int)relativeIndex.Value;
        }
        // 6. Else,
        else
        {
            // a. Let k be len + relativeIndex.
            k = len + (int)relativeIndex.Value;
        }

        // 7. If k < 0 or k ≥ len, return undefined.
        if (k < 0 || k >= len) return Undefined.The;

        // 8. Return the substring of S from k to k + 1.
        return S.Value.Substring(k, 1);
    }

    // 22.1.3.28 String.prototype.toLowerCase ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tolowercase
    private Completion toLowerCase(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Let O be ? RequireObjectCoercible(this value).
        var O = thisValue.RequireObjectCoercible(vm);
        if (O.IsAbruptCompletion()) return O;

        // 2. Let S be ? ToString(O).
        var S = O.Value.ToStringJS(vm);
        if (S.IsAbruptCompletion()) return S.Completion;

        // 3. Let sText be StringToCodePoints(S).
        var sText = S.Value.ToCharArray();

        // 4. Let lowerText be the result of toLowercase(sText), according to the Unicode Default Case Conversion algorithm.
        for (var i = 0; i < sText.Length; i++)
        {
            sText[i] = char.ToLower(sText[i]);
        }

        // 5. Let L be CodePointsToString(lowerText).
        var L = new string(sText);

        // 6. Return L.
        return L;
    }

    // 22.1.3.29 String.prototype.toString ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tostring
    private Completion toString(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Return ? ThisStringValue(this value).
        var result = ThisStringValue(vm, thisValue);
        if (result.IsAbruptCompletion()) return result.Completion;
        return result.Value;
    }

    // 22.1.3.35 String.prototype.valueOf ( ), https://tc39.es/ecma262/#sec-string.prototype.valueof
    private Completion valueOf(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Return ? ThisStringValue(this value).
        var result = ThisStringValue(vm, thisValue);
        if (result.IsAbruptCompletion()) return result.Completion;
        return result.Value;
    }

    // 22.1.3.35.1 ThisStringValue ( value ), https://tc39.es/ecma262/multipage/text-processing.html#sec-thisstringvalue
    private AbruptOr<String> ThisStringValue(VM vm, Value value)
    {
        // 1. If value is a String, return value.
        if (value.IsString()) return value.AsString();

        // 2. If value is an Object and value has a [[StringData]] internal slot, then
        if (value is StringObject stringObject)
        {
            // a. Let s be value.[[StringData]].
            var s = stringObject.StringData;

            // b. Assert: s is a String.

            // c. Return s.
            return s;
        }

        // 3. Throw a TypeError exception.
        return ThrowTypeError(vm, RuntimeErrorType.ThisIsNotAString);
    }
}
