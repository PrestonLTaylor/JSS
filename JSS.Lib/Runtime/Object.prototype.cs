using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.1.3 Properties of the Object Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-object-prototype-object
internal class ObjectPrototype : Object
{
    // The Object prototype object has a [[Prototype]] internal slot whose value is null.
    public ObjectPrototype() : base(null)
    {
    }

    public void Initialize(Realm realm, VM vm)
    {
        // 20.1.3.1 Object.prototype.constructor, The initial value of Object.prototype.constructor is %Object%.
        DataProperties.Add("constructor", new Property(realm.ObjectConstructor, new Attributes(true, false, true)));

        // 20.1.3.6 Object.prototype.toString ( ), https://tc39.es/ecma262/#sec-object.prototype.tostring
        var toStringBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, toString);
        DataProperties.Add("toString", new Property(toStringBuiltin, new Attributes(true, false, true)));
    }

    // 20.1.3.6 Object.prototype.toString ( ), https://tc39.es/ecma262/#sec-object.prototype.tostring
    private Completion toString(VM vm, Value? thisValue, List argumentList)
    {
        // 1. If the this value is undefined, return "[object Undefined]".
        if (thisValue is null || thisValue.IsUndefined()) return "[object Undefined]";

        // 2. If the this value is null, return "[object Null]".
        if (thisValue.IsNull()) return "[object Null]";

        // 3. Let O be ! ToObject(this value).
        var O = MUST(thisValue.ToObject(vm));

        // FIXME: 4. Let isArray be ? IsArray(O).
        // FIXME: 5. If isArray is true, let builtinTag be "Array".

        // FIXME: 6. Else if O has a [[ParameterMap]] internal slot, let builtinTag be "Arguments".

        // 7. Else if O has a [[Call]] internal method, let builtinTag be "Function".
        string builtinTag;
        if (O.HasInternalCall())
        {
            builtinTag = "Function";
        }
        // FIXME: 8. Else if O has an [[ErrorData]] internal slot, let builtinTag be "Error".
        // FIXME: 9. Else if O has a [[BooleanData]] internal slot, let builtinTag be "Boolean".
        // FIXME: 10. Else if O has a [[NumberData]] internal slot, let builtinTag be "Number".
        // FIXME: 11. Else if O has a [[StringData]] internal slot, let builtinTag be "String".
        // FIXME: 12. Else if O has a [[DateValue]] internal slot, let builtinTag be "Date".
        // FIXME: 13. Else if O has a [[RegExpMatcher]] internal slot, let builtinTag be "RegExp".
        // 14. Else, let builtinTag be "Object".
        else
        {
            builtinTag = "Object";
        }

        // 15. Let tag be ? Get(O, @@toStringTag).
        var getResult = Get(O, "\"Symbol.toStringTag\"");
        if (getResult.IsAbruptCompletion()) return getResult;

        // 16. If tag is not a String, set tag to builtinTag.
        string tag;
        if (!getResult.Value.IsString())
        {
            tag = builtinTag;
        }
        else
        {
            tag = getResult.Value.AsString();
        }

        // 17. Return the string-concatenation of "[object ", tag, and "]".
        return "[object " + tag + "]";
    }
}
