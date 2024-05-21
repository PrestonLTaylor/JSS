using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 21.1.1 The Number Constructor, https://tc39.es/ecma262/#sec-number-constructor
internal sealed class NumberConstructor : Object, ICallable, IConstructable
{
    // The Number constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public NumberConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(VM vm)
    {
        // 21.1.2.1 Number.EPSILON, The value of Number.EPSILON is the Number value for the magnitude of the difference between 1 and the smallest value greater than 1 that is representable as a Number value.
        DataProperties.Add("EPSILON", new(2.2204460492503130808472633361816E-16, new(false, false, false)));

        // 21.1.2.4 Number.isNaN ( number ), https://tc39.es/ecma262/multipage/numbers-and-dates.html#sec-number.isnan
        var isNaNBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, isNaN, 1, "isNaN");
        DataProperties.Add("isNaN", new(isNaNBuiltin, new(true, false, true)));

        // 21.1.2.6 Number.MAX_SAFE_INTEGER, The value of Number.MAX_SAFE_INTEGER is 9007199254740991𝔽 (𝔽(2**53 - 1)).
        DataProperties.Add("MAX_SAFE_INTEGER", new(Math.Pow(2, 53) - 1, new(false, false, false)));

        // 21.1.2.7 Number.MAX_VALUE, The value of Number.MAX_VALUE is the largest positive finite value of the Number type, which is approximately 1.7976931348623157 × 10**308.
        DataProperties.Add("MAX_VALUE", new(double.MaxValue, new(false, false, false)));

        // 21.1.2.8 Number.MIN_SAFE_INTEGER, The value of Number.MIN_SAFE_INTEGER is -9007199254740991𝔽 (𝔽(-(2**53 - 1))).
        DataProperties.Add("MIN_SAFE_INTEGER", new(-(Math.Pow(2, 53) - 1), new(false, false, false)));

        // 21.1.2.9 Number.MIN_VALUE, The value of Number.MIN_VALUE is the smallest positive value of the Number type, which is approximately 5 × 10 ** -324.
        DataProperties.Add("MIN_VALUE", new(5E-324, new(false, false, false)));

        // 21.1.2.10 Number.NaN, The value of Number.NaN is NaN.
        DataProperties.Add("NaN", new(double.NaN, new(false, false, false)));

        // 21.1.2.11 Number.NEGATIVE_INFINITY, The value of Number.NEGATIVE_INFINITY is -∞𝔽.
        DataProperties.Add("NEGATIVE_INFINITY", new(double.NegativeInfinity, new(false, false, false)));

        // 21.1.2.14 Number.POSITIVE_INFINITY, The value of Number.POSITIVE_INFINITY is +∞𝔽.
        DataProperties.Add("POSITIVE_INFINITY", new(double.PositiveInfinity, new(false, false, false)));
    }

    // 21.1.1.1 Number ( value ), https://tc39.es/ecma262/#sec-number-constructor-number-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList, Undefined.The);
    }

    // 21.1.1.1 Number ( value ), https://tc39.es/ecma262/#sec-number-constructor-number-value
    public Completion Construct(VM vm, List argumentsList, Object newTarget)
    {
        // 1. If value is present, then
        double n;
        if (argumentsList.Count != 0)
        {
            // a. Let prim be ? ToNumeric(value).
            var prim = argumentsList[0].ToNumeric(vm);
            if (prim.IsAbruptCompletion()) return prim;

            // FIXME: b. If prim is a BigInt, let n be 𝔽(ℝ(prim)).
            // c. Otherwise, let n be prim.
            n = prim.Value.AsNumber();
        }
        // 2. Else,
        else
        {
            // a. Let n be +0𝔽.
            n = 0;
        }

        // 3. If NewTarget is undefined, return n.
        if (newTarget.IsUndefined()) return n;

        // FIXME: 4. Let O be ? OrdinaryCreateFromConstructor(NewTarget, "%Number.prototype%", « [[NumberData]] »).
        // 5. Set O.[[NumberData]] to n.
        var O = new NumberObject(vm.ObjectPrototype, n);

        // 6. Return O.
        return O;
    }

    // 21.1.2.4 Number.isNaN ( number ), https://tc39.es/ecma262/multipage/numbers-and-dates.html#sec-number.isnan
    private Completion isNaN(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. If number is not a Number, return false.
        var number = argumentList[0];
        if (!number.IsNumber()) return false;

        // 2. If number is NaN, return true.
        // 3. Otherwise, return false.
        return double.IsNaN(number.AsNumber());
    }
}
