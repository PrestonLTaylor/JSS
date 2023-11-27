﻿using JSS.Lib.Execution;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JSS.Lib.AST.Values;

// FIXME: This is a very inefficient way of storing JS values.
// 6.1 ECMAScript Language Types, https://tc39.es/ecma262/#sec-ecmascript-language-types
internal abstract class Value
{
    virtual public bool IsEmpty() { return false; }
    virtual public bool IsUndefined() { return false; }
    virtual public bool IsNull() { return false; }
    virtual public bool IsBoolean() { return false; }
    virtual public bool IsString() { return false; }
    virtual public bool IsSymbol() { return false; }
    virtual public bool IsNumber() { return false; }
    virtual public bool IsBigInt() { return false; }
    virtual public bool IsObject() { return false; }

    // 6.2.5.5 GetValue ( V ), https://tc39.es/ecma262/#sec-getvalue
    public Completion GetValue()
    {
        // 1. If V is not a Reference Record, return V.
        return Completion.NormalCompletion(this);

        // FIXME: 2. If IsUnresolvableReference(V) is true, throw a ReferenceError exception.
        // FIXME: 3. If IsPropertyReference(V) is true, then
        // FIXME: a. Let baseObj be ? ToObject(V.[[Base]]).
        // FIXME: b. If IsPrivateReference(V) is true, then
        // FIXME: i. Return ? PrivateGet(baseObj, V.[[ReferencedName]]).
        // FIXME: c. Return ? baseObj.[[Get]](V.[[ReferencedName]], GetThisValue(V)).
        // FIXME: 4. Else,
        // FIXME: a. Let base be V.[[Base]].
        // FIXME: b. Assert: base is an Environment Record.
        // FIXME: c. Return ? base.GetBindingValue(V.[[ReferencedName]], V.[[Strict]]) (see 9.1).
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

    // 7.1.3 ToNumeric ( value ), https://tc39.es/ecma262/#sec-tonumeric
    public Completion ToNumeric(VM vm)
    {
        // 1. Let primValue be ? ToPrimitive(value, FIXME: NUMBER).
        var primValue = ToPrimitive();
        if (primValue.IsAbruptCompletion()) { return primValue; }

        // FIXME: 2. If primValue is a BigInt, return primValue.

        // 3. Return ? ToNumber(primValue).
        return primValue.Value.ToNumber(vm);
    }

    // 7.1.4 ToNumber ( argument ), https://tc39.es/ecma262/#sec-tonumber
    public Completion ToNumber(VM vm)
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
            var boolean = this as Boolean;
            var asNumber = new Number(boolean!.Value ? 1.0 : 0.0);
            return Completion.NormalCompletion(asNumber);
        }

        // FIXME: Implement StringToNumber instead of using double.Parse
        // 6. If argument is a String, return StringToNumber(argument).
        if (IsString())
        {
            try
            {
                var asString = this as String;
                var asNumber = new Number(double.Parse(asString!.Value));
                return Completion.NormalCompletion(asNumber);
            }
            catch (Exception)
            {
                return Completion.NormalCompletion(vm.NaN);
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
            var boolean = this as Boolean;
            var asString = new String(boolean!.Value ? "true" : "false");
            return Completion.NormalCompletion(asString);
        }

        // FIXME: Follow the spec instead of using C#'s ToString
        // 7. If argument is a Number, return Number::toString(argument, 10).
        if (IsNumber())
        {
            var number = this as Number;
            var asString = new String(number!.Value.ToString());
            return Completion.NormalCompletion(asString);
        }

        // FIXME: 8. If argument is a BigInt, return BigInt::toString(argument, 10).
        // FIXME: 9. Assert: argument is an Object.
        // FIXME: 10. Let primValue be ? ToPrimitive(argument, STRING).
        // FIXME: 11. Assert: primValue is not an Object.
        // FIXME: 12. Return ? ToString(primValue).
        throw new NotImplementedException();
    }

    // 7.2.13 IsLessThan ( x, y, LeftFirst )
    static public Completion IsLessThan(VM vm, Value x, Value y, bool leftFirst)
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
            var pxAsString = (px.Value as String)!;
            var lx = pxAsString!.Value.Length;

            // b. Let ly be the length of py.
            var pyAsString = (py.Value as String)!;
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
        var nx = px.Value.ToNumeric(vm);
        if (nx.IsAbruptCompletion()) return nx;

        // e. Let ny be ? ToNumeric(py).
        var ny = py.Value.ToNumeric(vm);
        if (ny.IsAbruptCompletion()) return ny;

        // FIXME: f. If Type(nx) is Type(ny), then

        // i. If nx is a Number, then
        // 1. Return Number::lessThan(nx, ny).
        var result = Number.LessThan((nx.Value as Number)!, (ny.Value as Number)!);
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
}
