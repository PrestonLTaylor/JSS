using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// FIXME: The Number prototype object is itself a Number object; it has a [[NumberData]] internal slot with the value +0𝔽.
// 21.1.3 Properties of the Number Prototype Object, https://tc39.es/ecma262/#sec-number.prototype
internal sealed class NumberPrototype : Object
{
	// The Number prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
	public NumberPrototype(ObjectPrototype prototype) : base(prototype)
	{
	}

	public void Initialize(Realm realm, VM vm)
	{
		// 21.1.3.1 Number.prototype.constructor, https://tc39.es/ecma262/#sec-number.prototype.constructor
		InternalDefineProperty("constructor", realm.NumberConstructor, new(true, false, true));

		// 21.1.3.6 Number.prototype.toString ( [ radix ] ), https://tc39.es/ecma262/#sec-number.prototype.tostring
		InternalDefineProperty(vm, "toString", 1, toString, new(true, false, true));

		// 21.1.3.7 Number.prototype.valueOf ( ), https://tc39.es/ecma262/#sec-number.prototype.valueof
		InternalDefineProperty(vm, "valueOf", 0, valueOf, new(true, false, true));
    }

	// 21.1.3.6 Number.prototype.toString ( [ radix ] ), https://tc39.es/ecma262/#sec-number.prototype.tostring
	private Completion toString(VM vm, Value thisValue, List argumentList, Object newTarget)
	{
		// 1. Let x be ? ThisNumberValue(this value).
		var x = ThisNumberValue(vm, thisValue);
		if (x.IsAbruptCompletion()) return x.Completion;

		// 2. If radix is undefined, let radixMV be 10.
		var radix = argumentList[0];
		int radixMV;
		if (radix.IsUndefined())
		{
			radixMV = 10;
		}
		// 3. Else, let radixMV be ? ToIntegerOrInfinity(radix).
		else
		{
			var toInteger = radix.ToIntegerOrInfinity(vm);
			if (toInteger.IsAbruptCompletion()) return toInteger.Completion;
			radixMV = (int)toInteger.Value;
		}

		// 4. If radixMV is not in the inclusive interval from 2 to 36, throw a RangeError exception.
		if (radixMV < 2 || radixMV > 36) return ThrowRangeError(vm, RuntimeErrorType.ArgumentOutOfRange, "radix", "2", "36");

		// FIXME: 5. Return Number::toString(x, radixMV).
		return x.Value.Value.ToString();
	}

	// 21.1.3.7 Number.prototype.valueOf ( ), https://tc39.es/ecma262/#sec-number.prototype.valueof
	private Completion valueOf(VM vm, Value thisValue, List argumentList, Object newTarget)
	{
        // 1. Return ? ThisNumberValue(this value).
        var result = ThisNumberValue(vm, thisValue);
        if (result.IsAbruptCompletion()) return result.Completion;
        return result.Value;
    }

    // 21.1.3.7.1 ThisNumberValue ( value ), https://tc39.es/ecma262/#sec-thisnumbervalue
    private AbruptOr<Number> ThisNumberValue(VM vm, Value value)
	{
		// 1. If value is a Number, return value.
		if (value.IsNumber()) return value.AsNumber();

		// 2. If value is an Object and value has a [[NumberData]] internal slot, then
		if (value is NumberObject numberObject)
		{
			// a. Let n be value.[[NumberData]].
			var n = numberObject.NumberData;

			// b. Assert: n is a Number.

			// c. Return n.
			return n;
		}

		// 3. Throw a TypeError exception.
		return ThrowTypeError(vm, RuntimeErrorType.ThisIsNotANumber);
	}
}
