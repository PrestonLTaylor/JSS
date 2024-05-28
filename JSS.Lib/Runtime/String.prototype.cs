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
        // 22.1.3.6 String.prototype.constructor, The initial value of String.prototype.constructor is %String%.
        DataProperties.Add("constructor", new(realm.StringConstructor, new(true, false, true)));

        // 22.1.3.28 String.prototype.toLowerCase ( ), https://tc39.es/ecma262/multipage/text-processing.html#sec-string.prototype.tolowercase
        var toLowerCaseBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, toLowerCase, 0, "toLowerCase");
        DataProperties.Add("toLowerCase", new(toLowerCaseBuiltin, new(true, false, true)));
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
}
