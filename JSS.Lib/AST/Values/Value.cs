﻿using JSS.Lib.Execution;
using System.Diagnostics;
using Environment = JSS.Lib.Execution.Environment;

namespace JSS.Lib.AST.Values;

internal enum ValueType
{
    Undefined,
    Null,
    Boolean,
    String,
    Symbol,
    Number,
    BigInt,
    Object,
    Function
}

// FIXME: This is a very inefficient way of storing JS values.
// 6.1 ECMAScript Language Types, https://tc39.es/ecma262/#sec-ecmascript-language-types
internal abstract class Value
{
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
    virtual public bool IsFunction() { return false; }

    public Reference AsReference()
    {
        Debug.Assert(IsReference());
        return (this as Reference)!;
    }

    public Environment AsEnvironment()
    {
        Debug.Assert(IsEnvironment());
        return (this as Environment)!;
    }

    public Boolean AsBoolean()
    {
        Debug.Assert(IsBoolean());
        return (this as Boolean)!;
    }

    public String AsString()
    {
        Debug.Assert(IsString());
        return (this as String)!;
    }

    public Number AsNumber()
    {
        Debug.Assert(IsNumber());
        return (this as Number)!;
    }

    public Object AsObject()
    {
        Debug.Assert(IsObject());
        return (this as Object)!;
    }

    public ICallable AsCallable()
    {
        Debug.Assert(HasInternalCall());
        return (this as ICallable)!;
    }

    public IConstructable AsConstructable()
    {
        Debug.Assert(HasInternalConstruct());
        return (this as IConstructable)!;
    }

    virtual public bool HasInternalCall() { return this is ICallable; }
    virtual public bool HasInternalConstruct() { return this is IConstructable; }

    abstract public ValueType Type();

    // 6.2.5.5 GetValue ( V ), https://tc39.es/ecma262/#sec-getvalue
    public Completion GetValue()
    {
        // 1. If V is not a Reference Record, return V.
        if (!IsReference())
        {
            return Completion.NormalCompletion(this);
        }

        // FIXME: Throw an ReferenceError Object
        // 2. If IsUnresolvableReference(V) is true, throw a ReferenceError exception.
        var asReference = AsReference();
        if (asReference.IsUnresolvableReference())
        {
            return Completion.ThrowCompletion(new String($"{asReference.ReferencedName} is not defined"));
        }

        // 3. If IsPropertyReference(V) is true, then
        if (asReference.IsPropertyReference())
        {
            // a. Let baseObj be ? ToObject(V.[[Base]]).
            var baseObj = asReference.Base!.ToObject();
            if (baseObj.IsAbruptCompletion()) return baseObj;

            // FIXME: b. If IsPrivateReference(V) is true, then
            // FIXME: i. Return ? PrivateGet(baseObj, V.[[ReferencedName]]).

            // c. Return ? baseObj.[[Get]](V.[[ReferencedName]], FIXME: GetThisValue(V)).
            var obj = baseObj.Value.AsObject();
            return obj.Get(asReference.ReferencedName, obj);
        }
        // 4. Else,
        else
        {
            // a. Let base be V.[[Base]].
            var @base = asReference.Base!;

            // b. Assert: base is an Environment Record.
            Debug.Assert(@base.IsEnvironment());

            // c. Return ? base.GetBindingValue(V.[[ReferencedName]], FIXME: V.[[Strict]]) (see 9.1).
            var environment = @base.AsEnvironment();
            return environment.GetBindingValue(asReference.ReferencedName, false);
        }
    }

    // 6.2.5.6 PutValue( V, W ), https://tc39.es/ecma262/#sec-putvalue
    public Completion PutValue(VM vm, Value W)
    {
        // 1. If V is not a Reference Record, throw a ReferenceError exception.
        if (!IsReference())
        {
            // FIXME: Throw an actual ReferenceError object
            return Completion.ThrowCompletion(new String("Tried to put a value into a non-reference."));
        }

        var reference = AsReference();

        // 2. If IsUnresolvableReference(V) is true, then
        if (reference.IsUnresolvableReference())
        {
            // FIXME: a. If V.[[Strict]] is true, throw a ReferenceError exception.

            // b. Let globalObj be GetGlobalObject().
            var globalObj = Realm.GetGlobalObject(vm);

            // c. Perform ? Set(globalObj, V.[[ReferencedName]], W, false).
            var setResult = Object.Set(globalObj, reference.ReferencedName, W, false);
            if (setResult.IsAbruptCompletion()) return setResult;

            // d. Return UNUSED.
            return Completion.NormalCompletion(Empty.The);
        }
        // 3. If IsPropertyReference(V) is true, then
        if (reference.IsPropertyReference())
        {
            // a. Let baseObj be ? ToObject(V.[[Base]]).
            var baseObj = reference.Base!.ToObject();

            // FIXME: b. If IsPrivateReference(V) is true, then
            // FIXME: i. Return ? PrivateSet(baseObj, V.[[ReferencedName]], W).

            // FIXME: c. Let succeeded be ? baseObj.[[Set]](V.[[ReferencedName]], W, FIXME: GetThisValue(V)).
            var obj = baseObj.Value.AsObject();
            var succeeded = obj.Set(reference.ReferencedName, W, obj);
            if (succeeded.IsAbruptCompletion()) return succeeded;

            // FIXME: d. If succeeded is false FIXME: (and V.[[Strict]] is true,) throw a TypeError exception.
            // e. Return UNUSED.
            return Completion.NormalCompletion(Empty.The);
        }
        // 4. Else,
        else
        {
            // a. Let base be V.[[Base]].
            var @base = reference.Base!;

            // b. Assert: base is an Environment Record.
            Debug.Assert(@base.IsEnvironment());

            // c. Return ? base.SetMutableBinding(V.[[ReferencedName]], W, FIXME: V.[[Strict]]) (see 9.1).
            var environment = @base.AsEnvironment();
            return environment.SetMutableBinding(reference.ReferencedName, W, false);
        }
    }

    // 7.1.1 ToPrimitive ( input FIXME: [ , preferredType ] ), https://tc39.es/ecma262/#sec-toprimitive
    public Completion ToPrimitive()
    {
        // FIXME: 1 .If input is an Object, then
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
        // FIXME:  vi. Throw a TypeError exception.
        // FIXME: c. If preferredType is not present, let preferredType be NUMBER.
        // FIXME: d. Return? OrdinaryToPrimitive(input, preferredType).

        // 2. Return input.
        return Completion.NormalCompletion(this);
    }

    // 7.1.2 ToBoolean ( argument ), https://tc39.es/ecma262/#sec-toboolean
    public Boolean ToBoolean()
    {
        // 1. If argument is a Boolean, return argument.
        if (IsBoolean())
        {
            return AsBoolean();
        }

        // 2. If argument is one of undefined, null, +0𝔽, -0𝔽, NaN, 0ℤ, or the empty String, return false.
        if (IsUndefined() || IsNull())
        {
            return new Boolean(false);
        }

        if (IsNumber())
        {
            var asNumber = AsNumber().Value;
            if (asNumber == 0 || double.IsNaN(asNumber))
            {
                return new Boolean(false);
            }
        }

        if (IsString())
        {
            var asString = AsString().Value;
            if (asString.Length == 0)
            {
                return new Boolean(false);
            }
        }

        // 3. NOTE: This step is replaced in section B.3.6.1.

        // 4. Return true.
        return new Boolean(true);
    }

    // 7.1.3 ToNumeric ( value ), https://tc39.es/ecma262/#sec-tonumeric
    public Completion ToNumeric()
    {
        // 1. Let primValue be ? ToPrimitive(value, FIXME: NUMBER).
        var primValue = ToPrimitive();
        if (primValue.IsAbruptCompletion()) { return primValue; }

        // FIXME: 2. If primValue is a BigInt, return primValue.

        // 3. Return ? ToNumber(primValue).
        return primValue.Value.ToNumber();
    }

    // 7.1.4 ToNumber ( argument ), https://tc39.es/ecma262/#sec-tonumber
    public Completion ToNumber()
    {
        // 1. If argument is a Number, return argument.
        if (IsNumber()) return Completion.NormalCompletion(this);

        // FIXME: 2. If argument is either a Symbol or a BigInt, throw a TypeError exception.

        // FIXME: 3. If argument is undefined, return NaN.

        // 4. If argument is either null or false, return FIXME: +0𝔽.
        if (IsNull()) return Completion.NormalCompletion(new Number(0.0));

        if (IsBoolean())
        {
            // 5. If argument is true, return 1𝔽.
            var boolean = AsBoolean();
            var asNumber = new Number(boolean.Value ? 1.0 : 0.0);
            return Completion.NormalCompletion(asNumber);
        }

        // FIXME: Implement StringToNumber instead of using double.Parse
        // 6. If argument is a String, return StringToNumber(argument).
        if (IsString())
        {
            try
            {
                var asString = AsString(); 
                var asNumber = new Number(double.Parse(asString.Value));
                return Completion.NormalCompletion(asNumber);
            }
            catch (Exception)
            {
                return Completion.NormalCompletion(Number.NaN);
            }
        }

        // FIXME: 7. Assert: argument is an Object.
        // FIXME: 8. Let primValue be ? ToPrimitive(argument, NUMBER).
        // FIXME: 9. Assert: primValue is not an Object.
        // 10. Return ? ToNumber(primValue).
        throw new NotImplementedException();
    }

    // 7.1.17 ToString ( argument ), https://tc39.es/ecma262/#sec-tostring
    public Completion ToStringJS()
    {
        // 1. If argument is a String, return argument.
        if (IsString()) return Completion.NormalCompletion(this);

        // FIXME: 2. If argument is a Symbol, throw a TypeError exception.

        // FIXME: 3. If argument is undefined, return "undefined".

        // 4. If argument is null, return "null".
        if (IsNull()) return Completion.NormalCompletion(new String("null"));

        // 5. If argument is true, return "true".
        // 6. If argument is false, return "false".
        if (IsBoolean())
        {
            var boolean = AsBoolean();
            var asString = new String(boolean.Value ? "true" : "false");
            return Completion.NormalCompletion(asString);
        }

        // FIXME: Follow the spec instead of using C#'s ToString
        // 7. If argument is a Number, return Number::toString(argument, 10).
        if (IsNumber())
        {
            var number = AsNumber();
            var asString = new String(number.Value.ToString());
            return Completion.NormalCompletion(asString);
        }

        // FIXME: 8. If argument is a BigInt, return BigInt::toString(argument, 10).
        // FIXME: 9. Assert: argument is an Object.
        // FIXME: 10. Let primValue be ? ToPrimitive(argument, STRING).
        // FIXME: 11. Assert: primValue is not an Object.
        // FIXME: 12. Return ? ToString(primValue).
        throw new NotImplementedException();
    }

    // 7.1.18 ToObject ( argument ), https://tc39.es/ecma262/#sec-toobject
    public Completion ToObject()
    {
        // Undefined, FIXME: Throw a TypeError exception.
        if (IsUndefined())
        {
            return Completion.ThrowCompletion(new String("Unable to convert undefined to an object"));
        }

        // Null, FIXME: Throw a TypeError exception.
        if (IsNull())
        {
            return Completion.ThrowCompletion(new String("Unable to convert null to an object"));
        }

        // FIXME: Implement the rest of the conversions

        // Object, Return argument.
        return Completion.NormalCompletion(this);
    }

    // 7.1.19 ToPropertyKey ( argument ), https://tc39.es/ecma262/#sec-topropertykey
    public Completion ToPropertyKey()
    {
        // 1. Let key be ? ToPrimitive(argument, string).
        var key = ToPrimitive();
        if (key.IsAbruptCompletion()) return key;

        // 2. If key is a Symbol, then
        if (key.Value.IsSymbol())
        {
            // a. Return key.
            return key;
        }

        // 3. Return ! ToString(key).
        var asString = key.Value.ToStringJS();
        Debug.Assert(asString.IsNormalCompletion());
        return asString;
    }

    // 7.2.3 IsCallable ( argument ), https://tc39.es/ecma262/#sec-iscallable
    public bool IsCallable()
    {
        // 1. If argument is not an Object, return false.
        if (!IsObject()) return false;

        // 2. If argument has a [[Call]] internal method, return true.
        if (HasInternalCall()) return true;

        // 3. Return false.
        return false;
    }

    // 7.2.4 IsConstructor ( argument ), https://tc39.es/ecma262/#sec-isconstructor
    public bool IsConstructor()
    {
        // 1. If argument is not an Object, return false.
        if (!IsObject()) return false;

        // 2. If argument has a [[Construct]] internal method, return true.
        if (HasInternalConstruct()) return true;

        // 3. Return false.
        return false;
    }

    // 7.2.12 SameValueNonNumber( x, y ), https://tc39.es/ecma262/#sec-samevaluenonnumber
    static public Boolean SameValueNonNumber(Value x, Value y)
    {
        // 1. Assert: Type(x) is Type(y).
        Debug.Assert(x.Type().Equals(y.Type()));

        // 2. If x is either null or undefined, return true.
        if (x.IsNull() || x.IsUndefined())
        {
            return new Boolean(true);
        }

        // FIXME: 3. If x is a BigInt, then
        // FIXME: a. Return BigInt::equal(x, y).

        // 4. If x is a String, then
        if (x.IsString())
        {
            // a. If x and y have the same length and the same code units in the same positions, return true; otherwise, return false.
            var xAsString = x.AsString().Value;
            var yAsString = y.AsString().Value;
            return new Boolean(xAsString == yAsString);
        }

        // 5. If x is a Boolean, then
        if (x.IsBoolean())
        {
            // 6. If x and y are both true or both false, return true; otherwise, return false.
            var xAsBoolean = x.AsBoolean().Value;
            var yAsBoolean = y.AsBoolean().Value;
            return new Boolean(xAsBoolean == yAsBoolean);
        }

        // FIXME: 6. NOTE: All other ECMAScript language values are compared by identity.
        // FIXME: 7. If x is y, return true; otherwise, return false.
        throw new NotImplementedException();
    }

    // 7.2.13 IsLessThan ( x, y, LeftFirst )
    static public Completion IsLessThan(Value x, Value y, bool leftFirst)
    {
        Completion px;
        Completion py;

        // 1. If LeftFirst is true, then
        if (leftFirst)
        {
            // a. Let px be ? ToPrimitive(x, FIXME: NUMBER).
            px = x.ToPrimitive();
            if (px.IsAbruptCompletion()) return px;

            // b. Let py be ? ToPrimitive(y, FIXME: NUMBER).
            py = y.ToPrimitive();
            if (py.IsAbruptCompletion()) return py;
        }
        // 2. Else,
        else
        {
            // a. NOTE: The order of evaluation needs to be reversed to preserve left to right evaluation.

            // b. Let py be ? ToPrimitive(y, FIXME: NUMBER).
            py = y.ToPrimitive();
            if (py.IsAbruptCompletion()) return py;

            // c. Let px be ? ToPrimitive(x, FIXME: NUMBER).
            px = x.ToPrimitive();
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
                    return Completion.NormalCompletion(new Boolean(true));
                }

                // iv. If cx > cy, return false.
                if (cx > cy)
                {
                    return Completion.NormalCompletion(new Boolean(false));
                }
            }

            // d. If lx < ly, return true. Otherwise, return false.
            return Completion.NormalCompletion(new Boolean(lx < ly));
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
        var nx = px.Value.ToNumeric();
        if (nx.IsAbruptCompletion()) return nx;

        // e. Let ny be ? ToNumeric(py).
        var ny = py.Value.ToNumeric();
        if (ny.IsAbruptCompletion()) return ny;

        // FIXME: f. If Type(nx) is Type(ny), then

        // i. If nx is a Number, then
        // 1. Return Number::lessThan(nx, ny).
        var result = Number.LessThan(nx.Value.AsNumber(), ny.Value.AsNumber());
        return Completion.NormalCompletion(result);

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
    static public Completion IsLooselyEqual(Value x, Value y)
    {
        // 1. If Type(x) is Type(y), then
        if (x.Type().Equals(y.Type()))
        {
            // a. Return IsStrictlyEqual(x, y).
            return Completion.NormalCompletion(IsStrictlyEqual(x, y));
        }

        // 2. If x is null and y is undefined, return true.
        if (x.IsNull() && y.IsUndefined())
        {
            return Completion.NormalCompletion(new Boolean(true));
        }

        // 3. If x is undefined and y is null, return true.
        if (x.IsUndefined() && y.IsNull())
        {
            return Completion.NormalCompletion(new Boolean(true));
        }

        // 4. NOTE: This step is replaced in section B.3.6.2.

        // 5. If x is a Number and y is a String, return ! IsLooselyEqual(x, ! ToNumber(y)).
        if (x.IsNumber() && y.IsString())
        {
            // FIXME: Maybe a MUST-like function for the asserts
            var yAsNumber = y.ToNumber();
            Debug.Assert(yAsNumber.IsNormalCompletion());

            var result = IsLooselyEqual(x, yAsNumber.Value);
            Debug.Assert(result.IsNormalCompletion());
            return result;
        }

        // 6. If x is a String and y is a Number, return ! IsLooselyEqual(! ToNumber(x), y).
        if (x.IsString() && y.IsNumber())
        {
            var xAsNumber = x.ToNumber();
            Debug.Assert(xAsNumber.IsNormalCompletion());

            var result = IsLooselyEqual(xAsNumber.Value, y);
            Debug.Assert(result.IsNormalCompletion());
            return result;
        }

        // FIXME: 7. If x is a BigInt and y is a String, then
        // FIXME: a. Let n be StringToBigInt(y).
        // FIXME: b. If n is undefined, return false.
        // FIXME: c. Return ! IsLooselyEqual(x, n).
        // FIXME: 8. If x is a String and y is a BigInt, return !IsLooselyEqual(y, x).

        // 9. If x is a Boolean, return !IsLooselyEqual(!ToNumber(x), y).
        if (x.IsBoolean())
        {
            var xAsNumber = x.ToNumber();
            Debug.Assert(xAsNumber.IsNormalCompletion());

            var result = IsLooselyEqual(xAsNumber.Value, y);
            Debug.Assert(result.IsNormalCompletion());
            return result;
        }

        // 10. If y is a Boolean, return !IsLooselyEqual(x, !ToNumber(y)).
        if (y.IsBoolean())
        {
            var yAsNumber = y.ToNumber();
            Debug.Assert(yAsNumber.IsNormalCompletion());

            var result = IsLooselyEqual(x, yAsNumber.Value);
            Debug.Assert(result.IsNormalCompletion());
            return result;
        }

        // FIXME: 11. If x is either a String, a Number, a BigInt, or a Symbol and y is an Object, return !IsLooselyEqual(x, ? ToPrimitive(y)).
        // FIXME: 12. If x is an Object and y is either a String, a Number, a BigInt, or a Symbol, return !IsLooselyEqual(? ToPrimitive(x), y).
        // FIXME: 13. If x is a BigInt and y is a Number, or if x is a Number and y is a BigInt, then
        // FIXME: a. If x is not finite or y is not finite, return false.
        // FIXME: b. If ℝ(x) = ℝ(y), return true; otherwise return false.

        // 14. Return false.
        return Completion.NormalCompletion(new Boolean(false));
    }

    // 7.2.15 IsStrictlyEqual ( x, y ), https://tc39.es/ecma262/#sec-isstrictlyequal
    static public Boolean IsStrictlyEqual(Value x, Value y)
    {
        // 1. If Type(x) is not Type(y), return false.
        if (!x.Type().Equals(y.Type()))
        {
            return new Boolean(false);
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
}
