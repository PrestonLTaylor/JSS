using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST.Values;

public enum ValueType
{
    Undefined,
    Null,
    Boolean,
    String,
    Symbol,
    Number,
    BigInt,
    Object,
    Function,
    Property
}

enum PreferredType
{
    STRING,
    NUMBER
}

// FIXME: This is a very inefficient way of storing JS values.
// 6.1 ECMAScript Language Types, https://tc39.es/ecma262/#sec-ecmascript-language-types
public abstract class Value
{
    public static implicit operator Value(bool value) => new Boolean(value);
    public static implicit operator Value(double value) => new Number(value);
    public static implicit operator Value(string value) => new String(value);

    virtual public bool IsEmpty() { return false; }
    virtual public bool IsReference() { return false; }
    virtual public bool IsEnvironment() { return false; }
    virtual public bool IsUndefined() { return false; }
    virtual public bool IsNull() { return false; }
    virtual public bool IsBoolean() { return false; }
    virtual public bool IsString() { return false; }
    virtual public bool IsSymbol() { return false; }
    virtual public bool IsNumber() { return false; }
    virtual public bool IsBigInt() { return false; }
    virtual public bool IsObject() { return false; }
    virtual public bool IsProperty() { return false; }

    public Reference AsReference()
    {
        Assert(IsReference(), $"AsReference called on a(n) {ToString()}.");
        return (this as Reference)!;
    }

    internal Environment AsEnvironment()
    {
        Assert(IsEnvironment(), $"AsEnvironment called on a(n) {ToString()}.");
        return (this as Environment)!;
    }

    public Boolean AsBoolean()
    {
        Assert(IsBoolean(), $"AsBoolean called on a(n) {ToString()}.");
        return (this as Boolean)!;
    }

    public String AsString()
    {
        Assert(IsString(), $"AsString called on a(n) {ToString()}.");
        return (this as String)!;
    }

    public Number AsNumber()
    {
        Assert(IsNumber(), $"AsNumber called on a(n) {ToString()}.");
        return (this as Number)!;
    }

    public Object AsObject()
    {
        Assert(IsObject(), $"AsObject called on a(n) {ToString()}.");
        return (this as Object)!;
    }

    internal Property AsProperty()
    {
        Assert(IsProperty(), $"AsProperty called on a(n) {ToString()}.");
        return (this as Property)!;
    }

    internal ICallable AsCallable()
    {
        Assert(HasInternalCall(), $"AsCallable called on a(n) {ToString()}.");
        return (this as ICallable)!;
    }

    internal IConstructable AsConstructable()
    {
        Assert(HasInternalConstruct(), $"AsConstructable called on a(n) {ToString()}.");
        return (this as IConstructable)!;
    }

    virtual public bool HasInternalCall() { return this is ICallable; }
    virtual public bool HasInternalConstruct() { return this is IConstructable; }

    abstract public ValueType Type();

    // 6.2.5.5 GetValue ( V ), https://tc39.es/ecma262/#sec-getvalue
    internal Completion GetValue(VM vm)
    {
        // 1. If V is not a Reference Record, return V.
        if (!IsReference())
        {
            return this;
        }

        // 2. If IsUnresolvableReference(V) is true, throw a ReferenceError exception.
        var asReference = AsReference();
        if (asReference.IsUnresolvableReference())
        {
            return ThrowReferenceError(vm, RuntimeErrorType.BindingNotDefined, asReference.ReferencedName);
        }

        // 3. If IsPropertyReference(V) is true, then
        if (asReference.IsPropertyReference())
        {
            // a. Let baseObj be ? ToObject(V.[[Base]]).
            var baseObj = asReference.Base!.ToObject(vm);
            if (baseObj.IsAbruptCompletion()) return baseObj;

            // FIXME: b. If IsPrivateReference(V) is true, then
            // FIXME: i. Return ? PrivateGet(baseObj, V.[[ReferencedName]]).

            // c. Return ? baseObj.[[Get]](V.[[ReferencedName]], FIXME: GetThisValue(V)).
            var obj = baseObj.Value;
            return obj.Get(asReference.ReferencedName, obj);
        }
        // 4. Else,
        else
        {
            // a. Let base be V.[[Base]].
            var @base = asReference.Base!;

            // b. Assert: base is an Environment Record.
            Assert(@base.IsEnvironment(), "b. Assert: base is an Environment Record.");

            // c. Return ? base.GetBindingValue(V.[[ReferencedName]], V.[[Strict]]) (see 9.1).
            var environment = @base.AsEnvironment();
            return environment.GetBindingValue(vm, asReference.ReferencedName, asReference.Strict);
        }
    }

    // 6.2.5.6 PutValue( V, W ), https://tc39.es/ecma262/#sec-putvalue
    internal Completion PutValue(VM vm, Value W)
    {
        // 1. If V is not a Reference Record, throw a ReferenceError exception.
        if (!IsReference())
        {
            return ThrowReferenceError(vm, RuntimeErrorType.PuttingValueInNonReference);
        }

        var reference = AsReference();

        // 2. If IsUnresolvableReference(V) is true, then
        if (reference.IsUnresolvableReference())
        {
            // a. If V.[[Strict]] is true, throw a ReferenceError exception.
            if (reference.Strict) return ThrowReferenceError(vm, RuntimeErrorType.FailedToSet, reference.ReferencedName);

            // b. Let globalObj be GetGlobalObject().
            var globalObj = Realm.GetGlobalObject(vm);

            // c. Perform ? Set(globalObj, V.[[ReferencedName]], W, false).
            var setResult = Object.Set(vm, globalObj, reference.ReferencedName, W, false);
            if (setResult.IsAbruptCompletion()) return setResult;

            // d. Return UNUSED.
            return Empty.The;
        }
        // 3. If IsPropertyReference(V) is true, then
        if (reference.IsPropertyReference())
        {
            // a. Let baseObj be ? ToObject(V.[[Base]]).
            var baseObj = reference.Base!.ToObject(vm);
            if (baseObj.IsAbruptCompletion()) return baseObj;

            // FIXME: b. If IsPrivateReference(V) is true, then
            // FIXME: i. Return ? PrivateSet(baseObj, V.[[ReferencedName]], W).

            // FIXME: c. Let succeeded be ? baseObj.[[Set]](V.[[ReferencedName]], W, FIXME: GetThisValue(V)).
            var obj = baseObj.Value;
            var succeeded = obj.Set(vm, reference.ReferencedName, W, obj);
            if (succeeded.IsAbruptCompletion()) return succeeded;

            // d. If succeeded is false and V.[[Strict]] is true, throw a TypeError exception.
            if (!succeeded.Value.AsBoolean() && reference.Strict) return ThrowTypeError(vm, RuntimeErrorType.FailedToSet, reference.ReferencedName);

            // e. Return UNUSED.
            return Empty.The;
        }
        // 4. Else,
        else
        {
            // a. Let base be V.[[Base]].
            var @base = reference.Base!;

            // b. Assert: base is an Environment Record.
            Assert(@base.IsEnvironment(), "b. Assert: base is an Environment Record.");

            // c. Return ? base.SetMutableBinding(V.[[ReferencedName]], W, V.[[Strict]]) (see 9.1).
            var environment = @base.AsEnvironment();
            return environment.SetMutableBinding(vm, reference.ReferencedName, W, reference.Strict);
        }
    }

    // 6.2.6.4 FromPropertyDescriptor ( Desc ), https://tc39.es/ecma262/#sec-frompropertydescriptor
    public Value FromPropertyDescriptor(VM vm)
    {
        // 1. If Desc is undefined, return undefined.
        if (IsUndefined()) return Undefined.The;
        var desc = AsProperty();

        // FIXME: 2. Let obj be OrdinaryObjectCreate(%Object.prototype%).
        var obj = new Object(vm.ObjectPrototype);

        // FIXME: 3. Assert: obj is an extensible ordinary object with no own properties.

        // FIXME: 4. If Desc has a [[Value]] field, then
        // a. Perform ! CreateDataPropertyOrThrow(obj, "value", Desc.[[Value]]).
        MUST(Object.CreateDataPropertyOrThrow(vm, obj, "value", desc.Value));

        // FIXME: 5. If Desc has a [[Writable]] field, then
        // a. Perform ! CreateDataPropertyOrThrow(obj, "writable", Desc.[[Writable]]).
        MUST(Object.CreateDataPropertyOrThrow(vm, obj, "writable", desc.Attributes.Writable));

        // FIXME: 6. If Desc has a [[Get]] field, then
        // FIXME: a. Perform ! CreateDataPropertyOrThrow(obj, "get", Desc.[[Get]]).
        // FIXME: 7. If Desc has a [[Set]] field, then
        // FIXME: a. Perform ! CreateDataPropertyOrThrow(obj, "set", Desc.[[Set]]).

        // FIXME: 8. If Desc has an [[Enumerable]] field, then
        // a. Perform ! CreateDataPropertyOrThrow(obj, "enumerable", Desc.[[Enumerable]]).
        MUST(Object.CreateDataPropertyOrThrow(vm, obj, "enumerable", desc.Attributes.Enumerable));

        // FIXME: 9. If Desc has a [[Configurable]] field, then
        // a. Perform ! CreateDataPropertyOrThrow(obj, "configurable", Desc.[[Configurable]]).
        MUST(Object.CreateDataPropertyOrThrow(vm, obj, "configurable", desc.Attributes.Configurable));

        // 10. Return obj.
        return obj;
    }

    // 6.2.6.5 ToPropertyDescriptor ( Obj ), https://tc39.es/ecma262/#sec-topropertydescriptor
    internal AbruptOr<Property> ToPropertyDescriptor(VM vm)
    {
        // 1. If Obj is not an Object, throw a TypeError exception.
        if (!IsObject()) return ThrowTypeError(vm, RuntimeErrorType.ThisIsNotAnObject);
        var obj = AsObject();

        // FIXME: 2. Let desc be a new Property Descriptor that initially has no fields.
        // FIXME: Property fields should be nullable
        var desc = new Property(null!, new(false, false, false));

        // 3. Let hasEnumerable be ? HasProperty(Obj, "enumerable").
        var hasEnumerable = Object.HasProperty(obj, "enumerable");
        if (hasEnumerable.IsAbruptCompletion()) return hasEnumerable;

        // 4. If hasEnumerable is true, then
        if (hasEnumerable.Value.AsBoolean())
        {
            // a. Let enumerable be ToBoolean(? Get(Obj, "enumerable")).
            var getResult = Object.Get(obj, "enumerable");
            if (getResult.IsAbruptCompletion()) return getResult;

            var enumerable = getResult.Value.ToBoolean();

            // b. Set desc.[[Enumerable]] to enumerable.
            desc.Attributes.Enumerable = enumerable;
        }

        // 5. Let hasConfigurable be ? HasProperty(Obj, "configurable").
        var hasConfigurable = Object.HasProperty(obj, "configurable");
        if (hasConfigurable.IsAbruptCompletion()) return hasConfigurable;

        // 6. If hasConfigurable is true, then
        if (hasConfigurable.Value.AsBoolean())
        {
            // a. Let configurable be ToBoolean(? Get(Obj, "configurable")).
            var getResult = Object.Get(obj, "configurable");
            if (getResult.IsAbruptCompletion()) return getResult;

            var configurable = getResult.Value.ToBoolean();

            // b. Set desc.[[Configurable]] to configurable.
            desc.Attributes.Configurable = configurable;
        }

        // 7. Let hasValue be ? HasProperty(Obj, "value").
        var hasValue = Object.HasProperty(obj, "value");
        if (hasValue.IsAbruptCompletion()) return hasValue;

        // 8. If hasValue is true, then
        if (hasValue.Value.AsBoolean())
        {
            // a. Let value be ? Get(Obj, "value").
            var getResult = Object.Get(obj, "value");
            if (getResult.IsAbruptCompletion()) return getResult;

            // b. Set desc.[[Value]] to value.
            desc.Value = getResult.Value;
        }

        // 9. Let hasWritable be ? HasProperty(Obj, "writable").
        var hasWritable = Object.HasProperty(obj, "writable");
        if (hasWritable.IsAbruptCompletion()) return hasWritable;

        // 10. If hasWritable is true, then
        if (hasWritable.Value.AsBoolean())
        {
            // a. Let writable be ToBoolean(? Get(Obj, "writable")).
            var getResult = Object.Get(obj, "writable");
            if (getResult.IsAbruptCompletion()) return getResult;

            var writable = getResult.Value.ToBoolean();

            // b. Set desc.[[Writable]] to writable.
            desc.Attributes.Writable = writable;
        }

        // FIXME: 11. Let hasGet be ? HasProperty(Obj, "get").
        // FIXME: 12. If hasGet is true, then
        // FIXME: a. Let getter be ? Get(Obj, "get").
        // FIXME: b. If IsCallable(getter) is false and getter is not undefined, throw a TypeError exception.
        // FIXME: c. Set desc.[[Get]] to getter.
        // FIXME: 13. Let hasSet be ? HasProperty(Obj, "set").
        // FIXME: 14. If hasSet is true, then
        // FIXME: a. Let setter be ? Get(Obj, "set").
        // FIXME: b. If IsCallable(setter) is false and setter is not undefined, throw a TypeError exception.
        // FIXME: c. Set desc.[[Set]] to setter.
        // FIXME: 15. If desc has a [[Get]] field or desc has a [[Set]] field, then
        // FIXME: a. If desc has a [[Value]] field or desc has a [[Writable]] field, throw a TypeError exception.

        // 16. Return desc.
        return desc;
    }

    // 7.1.1 ToPrimitive ( input [ , preferredType ] ), https://tc39.es/ecma262/#sec-toprimitive
    internal Completion ToPrimitive(VM vm, PreferredType? preferredType = null)
    {
        // 1. If input is an Object, then
        if (IsObject())
        {
            var O = AsObject();

            // FIXME: a. Let exoticToPrim be ? GetMethod(input, @@toPrimitive).
            // FIXME: b. If exoticToPrim is not undefined, then
            // FIXME: i. If preferredType is not present, then
            // FIXME: 1. Let hint be "default".
            // FIXME: ii. Else if preferredType is STRING, then
            // FIXME: 1. Let hint be "string".
            // FIXME: iii. Else,
            // FIXME: 1. Assert: preferredType is NUMBER.
            // FIXME: 2. Let hint be "number".
            // FIXME: iv. Let result be ? Call(exoticToPrim, input, « hint »).
            // FIXME: v. If result is not an Object, return result.
            // FIXME: vi. Throw a TypeError exception.

            // c. If preferredType is not present, let preferredType be NUMBER.
            preferredType ??= PreferredType.NUMBER;

            // d. Return ? OrdinaryToPrimitive(input, preferredType).
            return O.OrdinaryToPrimitive(vm, preferredType.Value);
        }

        // 2. Return input.
        return this;
    }

    // 7.1.2 ToBoolean ( argument ), https://tc39.es/ecma262/#sec-toboolean
    internal Boolean ToBoolean()
    {
        // 1. If argument is a Boolean, return argument.
        if (IsBoolean())
        {
            return AsBoolean();
        }

        // 2. If argument is one of undefined, null, +0𝔽, -0𝔽, NaN, 0ℤ, or the empty String, return false.
        if (IsUndefined() || IsNull())
        {
            return false;
        }

        if (IsNumber())
        {
            var asNumber = AsNumber().Value;
            if (asNumber == 0 || double.IsNaN(asNumber))
            {
                return false;
            }
        }

        if (IsString())
        {
            var asString = AsString().Value;
            if (asString.Length == 0)
            {
                return false;
            }
        }

        // 3. NOTE: This step is replaced in section B.3.6.1.

        // 4. Return true.
        return true;
    }

    // 7.1.3 ToNumeric ( value ), https://tc39.es/ecma262/#sec-tonumeric
    internal Completion ToNumeric(VM vm)
    {
        // 1. Let primValue be ? ToPrimitive(value, NUMBER).
        var primValue = ToPrimitive(vm, PreferredType.NUMBER);
        if (primValue.IsAbruptCompletion()) { return primValue; }

        // FIXME: 2. If primValue is a BigInt, return primValue.

        // 3. Return ? ToNumber(primValue).
        return primValue.Value.ToNumber(vm);
    }

    // 7.1.4 ToNumber ( argument ), https://tc39.es/ecma262/#sec-tonumber
    internal AbruptOr<double> ToNumber(VM vm)
    {
        // 1. If argument is a Number, return argument.
        if (IsNumber()) return AsNumber().Value;

        // FIXME: 2. If argument is either a Symbol or a BigInt, throw a TypeError exception.

        // 3. If argument is undefined, return NaN.
        if (IsUndefined()) return double.NaN;

        // 4. If argument is either null or false, return FIXME: +0𝔽.
        if (IsNull()) return 0.0;

        if (IsBoolean())
        {
            // 5. If argument is true, return 1𝔽.
            var boolean = AsBoolean();
            var asNumber = boolean.Value ? 1.0 : 0.0;
            return asNumber;
        }

        // FIXME: Implement StringToNumber instead of using double.Parse
        // 6. If argument is a String, return StringToNumber(argument).
        if (IsString())
        {
            try
            {
                var asString = AsString(); 
                var asNumber = double.Parse(asString.Value);
                return asNumber;
            }
            catch (Exception)
            {
                return double.NaN;
            }
        }

        // 7. Assert: argument is an Object.
        Assert(IsObject(), "7. Assert: argument is an Object.");

        // 8. Let primValue be ? ToPrimitive(argument, NUMBER).
        var primValue = ToPrimitive(vm, PreferredType.NUMBER);
        if (primValue.IsAbruptCompletion()) return primValue;

        // 9. Assert: primValue is not an Object.
        Assert(!primValue.Value.IsObject(), "9. Assert: primValue is not an Object.");

        // 10. Return ? ToNumber(primValue).
        return primValue.Value.ToNumber(vm);
    }

    // 7.1.5 ToIntegerOrInfinity ( argument ), https://tc39.es/ecma262/#sec-tointegerorinfinity
    internal AbruptOr<double> ToIntegerOrInfinity(VM vm)
    {
        // 1. Let number be ? ToNumber(argument).
        var number = ToNumber(vm);
        if (number.IsAbruptCompletion()) return number;

        // 2. If number is one of NaN, +0𝔽, FIXME: (or -0𝔽), return 0.
        if (number.Value is double.NaN or 0) return 0;

        // 3. If number is +∞𝔽, return +∞.
        if (number.Value == double.PositiveInfinity) return double.PositiveInfinity;

        // 4. If number is -∞𝔽, return -∞.
        if (number.Value == double.NegativeInfinity) return double.NegativeInfinity;

        // 5. Return truncate(ℝ(number)).
        return Math.Truncate(number.Value);
    }

    // 7.1.6 ToInt32 ( argument ), https://tc39.es/ecma262/#sec-toint32
    internal AbruptOr<int> ToInt32(VM vm)
    {
        const long TWO_TO_32 = (long)uint.MaxValue + 1;
        const long TWO_TO_31 = (long)int.MaxValue + 1;

        // 1. Let number be ? ToNumber(argument).
        var number = ToNumber(vm);
        if (number.IsAbruptCompletion()) return number.Completion;

        // 2. If number is FIXME: not finite or number is either +0𝔽 FIXME: or -0𝔽, return +0𝔽.
        var numberValue = number.Value;
        if (numberValue == 0.0) return 0;

        // 3. Let int be truncate(ℝ(number)).
        var @int = (long)numberValue;

        // 4. Let int32bit be int modulo 2**32.
        var int32bit = @int % TWO_TO_32;

        // 5. If int32bit ≥ 2**31, return 𝔽(int32bit - 2**32); otherwise return 𝔽(int32bit).
        if (int32bit >= TWO_TO_31) return (int)(int32bit - TWO_TO_32);
        return (int)int32bit;
    }

    // 7.1.7 ToUint32 ( argument ), https://tc39.es/ecma262/#sec-touint32
    internal AbruptOr<uint> ToUint32(VM vm)
    {
        const long TWO_TO_32 = (long)uint.MaxValue + 1;

        // 1. Let number be ? ToNumber(argument).
        var number = ToNumber(vm);
        if (number.IsAbruptCompletion()) return number.Completion;

        // 2. If number is FIXME: not finite or number is either +0𝔽 FIXME: or -0𝔽, return +0𝔽.
        var numberValue = number.Value;
        if (numberValue == 0.0) return 0;

        // 3. Let int be truncate(ℝ(number)).
        var @int = (long)numberValue;

        // 4. Let int32bit be int modulo 2**32.
        var int32bit = @int % TWO_TO_32;

        // 5. Return 𝔽(int32bit).
        return (uint)int32bit;
    }

    // 7.1.17 ToString ( argument ), https://tc39.es/ecma262/#sec-tostring
    internal AbruptOr<string> ToStringJS(VM vm)
    {
        // 1. If argument is a String, return argument.
        if (IsString()) return AsString().Value;

        // FIXME: 2. If argument is a Symbol, throw a TypeError exception.

        // 3. If argument is undefined, return "undefined".
        if (IsUndefined()) return "undefined";

        // 4. If argument is null, return "null".
        if (IsNull()) return "null";

        // 5. If argument is true, return "true".
        // 6. If argument is false, return "false".
        if (IsBoolean())
        {
            var boolean = AsBoolean();
            return boolean.Value ? "true" : "false";
        }

        // FIXME: Follow the spec instead of using C#'s ToString
        // 7. If argument is a Number, return Number::toString(argument, 10).
        if (IsNumber())
        {
            var number = AsNumber();
            return number.Value.ToString();
        }

        // FIXME: 8. If argument is a BigInt, return BigInt::toString(argument, 10).

        // 9. Assert: argument is an Object.
        Assert(IsObject(), "9. Assert: argument is an Object.");

        // 10. Let primValue be ? ToPrimitive(argument, STRING).
        var primValue = ToPrimitive(vm, PreferredType.STRING);
        if (primValue.IsAbruptCompletion()) return primValue;

        // 11. Assert: primValue is not an Object.
        Assert(!primValue.Value.IsObject(), "11. Assert: primValue is not an Object.");

        // 12. Return ? ToString(primValue).
        return primValue.Value.ToStringJS(vm);
    }

    // 7.1.18 ToObject ( argument ), https://tc39.es/ecma262/#sec-toobject
    internal AbruptOr<Object> ToObject(VM vm)
    {
        // Undefined, Throw a TypeError exception.
        if (IsUndefined())
        {
            return ThrowTypeError(vm, RuntimeErrorType.UnableToConvertToObject, "undefined");
        }

        // Null, Throw a TypeError exception.
        if (IsNull())
        {
            return ThrowTypeError(vm, RuntimeErrorType.UnableToConvertToObject, "null");
        }

        // Boolean, Return a new Boolean object whose [[BooleanData]] internal slot is set to argument.
        if (IsBoolean())
        {
            return new BooleanObject(vm.BooleanPrototype, AsBoolean());
        }

        // Number, Return a new Number object whose [[NumberData]] internal slot is set to argument.
        if (IsNumber())
        {
            return new NumberObject(vm.ObjectPrototype, AsNumber());
        }

        // String, Return a new String object whose [[StringData]] internal slot is set to argument.
        if (IsString())
        {
            return new StringObject(vm, AsString(), vm.StringPrototype);
        }

        // FIXME: Implement the rest of the conversions

        // Object, Return argument.
        return AsObject();
    }

    // 7.1.19 ToPropertyKey ( argument ), https://tc39.es/ecma262/#sec-topropertykey
    internal Completion ToPropertyKey(VM vm)
    {
        // 1. Let key be ? ToPrimitive(argument, string).
        var key = ToPrimitive(vm, PreferredType.STRING);
        if (key.IsAbruptCompletion()) return key;

        // 2. If key is a Symbol, then
        if (key.Value.IsSymbol())
        {
            // a. Return key.
            return key;
        }

        // 3. Return ! ToString(key).
        return MUST(key.Value.ToStringJS(vm));
    }

    // 7.2.1 RequireObjectCoercible ( argument ), https://tc39.es/ecma262/multipage/abstract-operations.html#sec-requireobjectcoercible
    internal Completion RequireObjectCoercible(VM vm)
    {
        // Undefined, Throw a TypeError exception.
        if (IsUndefined()) return ThrowTypeError(vm, RuntimeErrorType.UnableToConvertToObject, "undefined");

        // Null, Throw a TypeError exception.
        if (IsNull()) return ThrowTypeError(vm, RuntimeErrorType.UnableToConvertToObject, "null");

        // NOTE: Every type asides from undefined and null just return the themselves.
        // Return argument.
        return this;
    }

    // 7.2.2 IsArray ( argument ), https://tc39.es/ecma262/#sec-isarray
    internal bool IsArray()
    {
        // 1. If argument is not an Object, return false.
        if (!IsObject()) return false;

        // 2. If argument is an Array exotic object, return true.
        if (this is Array) return true;

        // FIXME: 3. If argument is a Proxy exotic object, then
        // FIXME: a. Perform ? ValidateNonRevokedProxy(argument).
        // FIXME: b. Let proxyTarget be argument.[[ProxyTarget]].
        // FIXME: c. Return ? IsArray(proxyTarget).

        // 4. Return false.
        return false;
    }

    // 7.2.3 IsCallable ( argument ), https://tc39.es/ecma262/#sec-iscallable
    internal bool IsCallable()
    {
        // 1. If argument is not an Object, return false.
        if (!IsObject()) return false;

        // 2. If argument has a [[Call]] internal method, return true.
        if (HasInternalCall()) return true;

        // 3. Return false.
        return false;
    }

    // 7.2.4 IsConstructor ( argument ), https://tc39.es/ecma262/#sec-isconstructor
    internal bool IsConstructor()
    {
        // 1. If argument is not an Object, return false.
        if (!IsObject()) return false;

        // 2. If argument has a [[Construct]] internal method, return true.
        if (HasInternalConstruct()) return true;

        // 3. Return false.
        return false;
    }

    // 7.2.9 SameValue ( x, y ), https://tc39.es/ecma262/#sec-samevalue
    static internal Boolean SameValue(Value x, Value y)
    {
        // 1. If Type(x) is not Type(y), return false.
        if (x.Type() != y.Type()) return false;

        // FIXME: 2. If x is a Number, then
        // FIXME: a. Return Number::sameValue(x, y).

        // 3. Return SameValueNonNumber(x, y).
        return SameValueNonNumber(x, y);
    }

    // 7.2.12 SameValueNonNumber( x, y ), https://tc39.es/ecma262/#sec-samevaluenonnumber
    static internal Boolean SameValueNonNumber(Value x, Value y)
    {
        // 1. Assert: Type(x) is Type(y).
        Assert(x.Type().Equals(y.Type()), "1. Assert: Type(x) is Type(y).");

        // 2. If x is either null or undefined, return true.
        if (x.IsNull() || x.IsUndefined())
        {
            return true;
        }

        // FIXME: 3. If x is a BigInt, then
        // FIXME: a. Return BigInt::equal(x, y).

        // 4. If x is a String, then
        if (x.IsString())
        {
            // a. If x and y have the same length and the same code units in the same positions, return true; otherwise, return false.
            var xAsString = x.AsString().Value;
            var yAsString = y.AsString().Value;
            return xAsString == yAsString;
        }

        // 5. If x is a Boolean, then
        if (x.IsBoolean())
        {
            // 6. If x and y are both true or both false, return true; otherwise, return false.
            var xAsBoolean = x.AsBoolean().Value;
            var yAsBoolean = y.AsBoolean().Value;
            return xAsBoolean == yAsBoolean;
        }

        // NOTE: x and y should be reference types, comparing values by identity is the same as the reference equality between the two values
        // 6. NOTE: All other ECMAScript language values are compared by identity.
        // 7. If x is y, return true; otherwise, return false.
        return ReferenceEquals(x, y);
    }

    // 7.2.13 IsLessThan ( x, y, LeftFirst ), https://tc39.es/ecma262/#sec-islessthan
    static internal Completion IsLessThan(VM vm, Value x, Value y, bool leftFirst)
    {
        Completion px;
        Completion py;

        // 1. If LeftFirst is true, then
        if (leftFirst)
        {
            // a. Let px be ? ToPrimitive(x, NUMBER).
            px = x.ToPrimitive(vm, PreferredType.NUMBER);
            if (px.IsAbruptCompletion()) return px;

            // b. Let py be ? ToPrimitive(y, NUMBER).
            py = y.ToPrimitive(vm, PreferredType.NUMBER);
            if (py.IsAbruptCompletion()) return py;
        }
        // 2. Else,
        else
        {
            // a. NOTE: The order of evaluation needs to be reversed to preserve left to right evaluation.

            // b. Let py be ? ToPrimitive(y, NUMBER).
            py = y.ToPrimitive(vm, PreferredType.NUMBER);
            if (py.IsAbruptCompletion()) return py;

            // c. Let px be ? ToPrimitive(x, NUMBER).
            px = x.ToPrimitive(vm, PreferredType.NUMBER);
            if (px.IsAbruptCompletion()) return px;
        }

        // 3. If px is a String and py is a String, then
        if (px.Value.IsString() && py.Value.IsString())
        {
            // a. Let lx be the length of px.
            var pxAsString = px.Value.AsString();
            var lx = pxAsString!.Value.Length;

            // b. Let ly be the length of py.
            var pyAsString = py.Value.AsString();
            var ly = pyAsString!.Value.Length;

            // c. For each integer i such that 0 ≤ i < min(lx, ly), in ascending order, do
            for (int i = 0; i < Math.Min(lx, ly); ++i)
            {
                // i. Let cx be the numeric value of the code unit at index i within px.
                var cx = pxAsString.Value[i];

                // ii. Let cy be the numeric value of the code unit at index i within py.
                var cy = pyAsString.Value[i];

                // iii. If cx < cy, return true.
                if (cx < cy)
                {
                    return true;
                }

                // iv. If cx > cy, return false.
                if (cx > cy)
                {
                    return false;
                }
            }

            // d. If lx < ly, return true. Otherwise, return false.
            return lx < ly;
        }

        // 4. Else,
        // FIXME: a. If px is a BigInt and py is a String, then
        // FIXME: i. Let ny be StringToBigInt(py).
        // FIXME: ii. If ny is undefined, return undefined.
        // FIXME: iii. Return BigInt::lessThan(px, ny).
        // FIXME: b. If px is a String and py is a BigInt, then
        // FIXME: i. Let nx be StringToBigInt(px).
        // FIXME: ii. If nx is undefined, return undefined.
        // FIXME: iii. Return BigInt::lessThan(nx, py).

        // c. NOTE: Because px and py are primitive values, evaluation order is not important.

        // d. Let nx be ? ToNumeric(px).
        var nx = px.Value.ToNumeric(vm);
        if (nx.IsAbruptCompletion()) return nx;

        // e. Let ny be ? ToNumeric(py).
        var ny = py.Value.ToNumeric(vm);
        if (ny.IsAbruptCompletion()) return ny;

        // FIXME: f. If Type(nx) is Type(ny), then

        // i. If nx is a Number, then
        // 1. Return Number::lessThan(nx, ny).
        var result = Number.LessThan(nx.Value.AsNumber(), ny.Value.AsNumber());
        return result;

        // FIXME: ii. Else,
        // FIXME: 1. Assert: nx is a BigInt.
        // FIXME: 2. Return BigInt::lessThan(nx, ny).
        // FIXME: g. Assert: nx is a BigInt and ny is a Number, or nx is a Number and ny is a BigInt.
        // FIXME: h. If nx or ny is NaN, return undefined.
        // FIXME: i. If nx is -∞𝔽 or ny is +∞𝔽, return true.
        // FIXME: j. If nx is +∞𝔽 or ny is -∞𝔽, return false.
        // FIXME: k. If ℝ(nx) < ℝ(ny), return true; otherwise return false.
    }

    // 7.2.14 IsLooselyEqual ( x, y ), https://tc39.es/ecma262/#sec-islooselyequal
    static internal Completion IsLooselyEqual(VM vm, Value x, Value y)
    {
        // 1. If Type(x) is Type(y), then
        if (x.Type().Equals(y.Type()))
        {
            // a. Return IsStrictlyEqual(x, y).
            return IsStrictlyEqual(x, y);
        }

        // 2. If x is null and y is undefined, return true.
        if (x.IsNull() && y.IsUndefined())
        {
            return true;
        }

        // 3. If x is undefined and y is null, return true.
        if (x.IsUndefined() && y.IsNull())
        {
            return true;
        }

        // 4. NOTE: This step is replaced in section B.3.6.2.

        // 5. If x is a Number and y is a String, return ! IsLooselyEqual(x, ! ToNumber(y)).
        if (x.IsNumber() && y.IsString())
        {
            return MUST(IsLooselyEqual(vm, x, MUST(y.ToNumber(vm))));
        }

        // 6. If x is a String and y is a Number, return ! IsLooselyEqual(! ToNumber(x), y).
        if (x.IsString() && y.IsNumber())
        {
            return MUST(IsLooselyEqual(vm, MUST(x.ToNumber(vm)), y));
        }

        // FIXME: 7. If x is a BigInt and y is a String, then
        // FIXME: a. Let n be StringToBigInt(y).
        // FIXME: b. If n is undefined, return false.
        // FIXME: c. Return ! IsLooselyEqual(x, n).
        // FIXME: 8. If x is a String and y is a BigInt, return !IsLooselyEqual(y, x).

        // 9. If x is a Boolean, return !IsLooselyEqual(!ToNumber(x), y).
        if (x.IsBoolean())
        {
            var xAsNumber = MUST(x.ToNumber(vm));
            return MUST(IsLooselyEqual(vm, xAsNumber, y));
        }

        // 10. If y is a Boolean, return !IsLooselyEqual(x, !ToNumber(y)).
        if (y.IsBoolean())
        {
            var yAsNumber = MUST(y.ToNumber(vm));
            return MUST(IsLooselyEqual(vm, x, yAsNumber));
        }

        // FIXME: 11. If x is either a String, a Number, a BigInt, or a Symbol and y is an Object, return !IsLooselyEqual(x, ? ToPrimitive(y)).
        // FIXME: 12. If x is an Object and y is either a String, a Number, a BigInt, or a Symbol, return !IsLooselyEqual(? ToPrimitive(x), y).
        // FIXME: 13. If x is a BigInt and y is a Number, or if x is a Number and y is a BigInt, then
        // FIXME: a. If x is not finite or y is not finite, return false.
        // FIXME: b. If ℝ(x) = ℝ(y), return true; otherwise return false.

        // 14. Return false.
        return false;
    }

    // 7.2.15 IsStrictlyEqual ( x, y ), https://tc39.es/ecma262/#sec-isstrictlyequal
    static internal Boolean IsStrictlyEqual(Value x, Value y)
    {
        // 1. If Type(x) is not Type(y), return false.
        if (!x.Type().Equals(y.Type()))
        {
            return false;
        }

        // 2. If x is a Number, then
        if (x.IsNumber())
        {
            // a. Return Number::equal(x, y).
            return Number.Equal(x.AsNumber(), y.AsNumber());
        }

        // 3. Return SameValueNonNumber(x, y).
        return SameValueNonNumber(x, y);
    }

    // 7.1.20 ToLength ( argument ), https://tc39.es/ecma262/#sec-tolength
    internal AbruptOr<double> ToLength(VM vm)
    {
        // 1. Let len be ? ToIntegerOrInfinity(argument).
        var len = ToIntegerOrInfinity(vm);

        // 2. If len ≤ 0, return +0𝔽.
        if (len.Value <= 0) return 0;

        // 3. Return 𝔽(min(len, 2**53 - 1)).
        return Math.Min(len.Value, Math.Pow(2, 53) - 1);
    }

    // 7.3.21 OrdinaryHasInstance ( C, O ), https://tc39.es/ecma262/#sec-ordinaryhasinstance
    internal Completion OrdinaryHasInstance(VM vm, Value O)
    {
        // 1. If IsCallable(C) is false, return false.
        if (!IsCallable()) return false;

        // FIXME: 2. If C has a [[BoundTargetFunction]] internal slot, then
        // FIXME: a. Let BC be C.[[BoundTargetFunction]].
        // FIXME: b. Return ? InstanceofOperator(O, BC).

        // 3. If O is not an Object, return false.
        if (!O.IsObject()) return false;

        // 4. Let P be ? Get(C, "prototype").
        var C = AsObject();
        var P = Object.Get(C, "prototype");
        if (P.IsAbruptCompletion()) return P;

        // 5. If P is not an Object, throw a TypeError exception.
        if (!P.Value.IsObject()) return ThrowTypeError(vm, RuntimeErrorType.InstanceOfConstructorPrototypeIsNotAnObject, P.Value.Type());

        // 6. Repeat,
        Object? obj = O.AsObject();
        while (true)
        {
            // a. Set O to ? O.[[GetPrototypeOf]]().
            obj = obj.AsObject().GetPrototypeOf();

            // b. If O is null, return false.
            if (obj is null) return false;

            // c. If SameValue(P, O) is true, return true.
            if (SameValue(P.Value, obj)) return true;
        }
    }
}
