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

        // 22.1.3.2 String.prototype.charAt ( pos ), https://tc39.es/ecma262/#sec-string.prototype.charat
        InternalDefineProperty(vm, "charAt", 1, charAt, new(true, false, true));

        // 22.1.3.3 String.prototype.charCodeAt ( pos ), https://tc39.es/ecma262/#sec-string.prototype.charcodeat
        InternalDefineProperty(vm, "charCodeAt", 1, charCodeAt, new(true, false, true));

        // 22.1.3.6 String.prototype.constructor, The initial value of String.prototype.constructor is %String%.
        InternalDefineProperty("constructor", realm.StringConstructor, new(true, false, true));

        // 22.1.3.28 String.prototype.toLowerCase ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tolowercase
        InternalDefineProperty(vm, "toLowerCase", 0, toLowerCase, new(true, false, true));

        // 22.1.3.29 String.prototype.toString ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tostring
        InternalDefineProperty(vm, "toString", 0, toString, new(true, false, true));

        // 22.1.3.30 String.prototype.toUpperCase ( ), https://tc39.es/ecma262/#sec-string.prototype.touppercase
        InternalDefineProperty(vm, "toUpperCase", 0, toUpperCase, new(true, false, true));

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

    // 22.1.3.2 String.prototype.charAt ( pos ), https://tc39.es/ecma262/#sec-string.prototype.charat
    private Completion charAt(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Let O be ? RequireObjectCoercible(this value).
        var O = thisValue.RequireObjectCoercible(vm);
        if (O.IsAbruptCompletion()) return O;

        // 2. Let S be ? ToString(O).
        var S = O.Value.ToStringJS(vm);
        if (S.IsAbruptCompletion()) return S.Completion;

        // 3. Let position be ? ToIntegerOrInfinity(pos).
        var position = argumentList[0].ToIntegerOrInfinity(vm);
        if (position.IsAbruptCompletion()) return position.Completion;

        // 4. Let size be the length of S.
        var size = S.Value.Length;

        // 5. If position < 0 or position ≥ size, return the empty String.
        if (position.Value < 0 || position.Value >= size) return "";

        // 6. Return the substring of S from position to position + 1.
        return S.Value.Substring((int)position.Value, 1);
    }

    // 22.1.3.3 String.prototype.charCodeAt ( pos ), https://tc39.es/ecma262/#sec-string.prototype.charcodeat
    private Completion charCodeAt(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // 1. Let O be ? RequireObjectCoercible(this value).
        var O = thisValue.RequireObjectCoercible(vm);
        if (O.IsAbruptCompletion()) return O;

        // 2. Let S be ? ToString(O).
        var S = O.Value.ToStringJS(vm);
        if (S.IsAbruptCompletion()) return S.Completion;

        // 3. Let position be ? ToIntegerOrInfinity(pos).
        var position = argumentList[0].ToIntegerOrInfinity(vm);
        if (position.IsAbruptCompletion()) return position.Completion;

        // 4. Let size be the length of S.
        var size = S.Value.Length;

        // 5. If position < 0 or position ≥ size, return NaN.
        if (position.Value < 0 || position.Value >= size) return double.NaN;

        // 6. Return the Number value for the numeric value of the code unit at index position within the String S.
        return S.Value[(int)position.Value];
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

    // 22.1.3.30 String.prototype.toUpperCase ( ), https://tc39.es/ecma262/#sec-string.prototype.touppercase
    private Completion toUpperCase(VM vm, Value thisValue, List argumentList, Object newTarget)
    {
        // It behaves in exactly the same way as String.prototype.toLowerCase, except that the String is mapped using the toUppercase algorithm of the Unicode Default Case Conversion.
        // NOTE: I've kept the steps from toLowerCase but changed toLowercase to toUppercase in step 4.

        // 1. Let O be ? RequireObjectCoercible(this value).
        var O = thisValue.RequireObjectCoercible(vm);
        if (O.IsAbruptCompletion()) return O;

        // 2. Let S be ? ToString(O).
        var S = O.Value.ToStringJS(vm);
        if (S.IsAbruptCompletion()) return S.Completion;

        // 3. Let sText be StringToCodePoints(S).
        var sText = S.Value.ToCharArray();

        // 4. Let lowerText be the result of toUppercase(sText), according to the Unicode Default Case Conversion algorithm.
        for (var i = 0; i < sText.Length; i++)
        {
            sText[i] = char.ToUpper(sText[i]);
        }

        // 5. Let L be CodePointsToString(lowerText).
        var L = new string(sText);

        // 6. Return L.
        return L;
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
