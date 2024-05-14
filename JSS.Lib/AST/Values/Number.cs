using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

public sealed class Number : Value
{
    internal Number(double value)
    {
        Value = value;
    }

    public static implicit operator Number(double value) => new(value);
    public static implicit operator double(Number number) => number.Value;

    public override bool IsNumber() { return true; }
    override public ValueType Type() { return ValueType.Number; }

    override public bool Equals(object? obj)
    {
        if (obj is not Number rhs) return false;
        return Value.Equals(rhs.Value);
    }

    override public int GetHashCode()
    {
        return Value.GetHashCode();
    }

    // 6.1.6.1.1 Number::unaryMinus ( x ), https://tc39.es/ecma262/#sec-numeric-types-number-unaryMinus
    static internal Number UnaryMinus(Number x)
    {
        // 1. If x is NaN, return NaN.
        if (double.IsNaN(x.Value)) return x;

        // 2. Return the result of negating x; that is, compute a Number with the same magnitude but opposite sign.
        return -x.Value;
    }

    // 6.1.6.1.2 Number::bitwiseNOT ( x ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseNOT
    static internal Number BitwiseNOT(VM vm, Number x)
    {
        // 1. Let oldValue be ! ToInt32(x).
        var oldValue = MUST(x.ToInt32(vm));

        // 2. Return the result of applying bitwise complement to oldValue.
        // The mathematical value of the result is exactly representable as a 32-bit two's complement bit string.
        return ~oldValue;
    }

    // 6.1.6.1.3 Number::exponentiate ( base, exponent ), https://tc39.es/ecma262/#sec-numeric-types-number-exponentiate
    static internal Number Exponentiate(VM _, Number expBase, Number exponent)
    {
        // 1. If exponent is NaN, return NaN.
        if (double.IsNaN(exponent)) return double.NaN;

        // FIXME: 2. If exponent is either + 0𝔽 or - 0𝔽, return 1𝔽.

        // 3. If base is NaN, return NaN.
        if (double.IsNaN(expBase)) return double.NaN;

        // 4. If base is +∞𝔽, then
        if (expBase.Value == double.PositiveInfinity)
        {
            // a. If exponent > +0𝔽, return +∞𝔽. Otherwise, return +0𝔽.
            return exponent.Value > 0 ? double.PositiveInfinity : 0;
        }

        // 5. If base is -∞𝔽, then
        if (expBase.Value == double.NegativeInfinity)
        {
            // a. If exponent > +0𝔽, then
            if (exponent.Value > 0)
            {
                // i. If exponent is an odd integral Number, return -∞𝔽. Otherwise, return +∞𝔽.
                return double.IsOddInteger(exponent.Value) ? double.NegativeInfinity : double.PositiveInfinity;
            }
            // b. Else,
            else
            {
                // FIXME: i. If exponent is an odd integral Number, return -0𝔽. Otherwise, return +0𝔽.
                return 0;
            }
        }

        // FIXME: 6. If base is +0𝔽, then
        // FIXME: a. If exponent > +0𝔽, return +0𝔽. Otherwise, return +∞𝔽.
        // FIXME: 7. If base is -0𝔽, then
        // FIXME: a. If exponent > +0𝔽, then
        // FIXME: i. If exponent is an odd integral Number, return -0𝔽. Otherwise, return +0𝔽.
        // FIXME: b. Else,
        // FIXME: i. If exponent is an odd integral Number, return -∞𝔽. Otherwise, return +∞𝔽.
        // FIXME: 8. Assert: base is finite and is neither + 0𝔽 nor - 0𝔽.

        // 9. If exponent is +∞𝔽, then
        if (exponent.Value == double.PositiveInfinity)
        {
            // a. If abs(ℝ(base)) > 1, return +∞𝔽.
            var absBase = Math.Abs(expBase.Value);
            if (absBase > 1) return double.PositiveInfinity;

            // b. If abs(ℝ(base)) = 1, return NaN.
            if (absBase == 1) return double.NaN;

            // c. If abs(ℝ(base)) < 1, return +0𝔽.
            if (absBase < 1) return 0;
        }

        // 10. If exponent is -∞𝔽, then
        if (exponent.Value == double.PositiveInfinity)
        {
            // a. If abs(ℝ(base)) > 1, return +0𝔽.
            var absBase = Math.Abs(expBase.Value);
            if (absBase > 1) return 0;

            // b. If abs(ℝ(base)) = 1, return NaN.
            if (absBase == 1) return double.NaN;

            // c. If abs(ℝ(base)) < 1, return +∞𝔽.
            if (absBase < 1) return double.PositiveInfinity;
        }

        // FIXME: 11. Assert: exponent is finite and is neither + 0𝔽 nor - 0𝔽.
        // FIXME: 12. If base < -0𝔽 and exponent is not an integral Number, return NaN.

        // 13. Return an implementation-approximated Number value representing the result of raising ℝ(base) to the ℝ(exponent) power.
        return Math.Pow(expBase.Value, exponent.Value);
    }

    // 6.1.6.1.4 Number::multiply ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-multiply
    static internal Number Multiply(VM _, Number x, Number y)
    {
        // 1. If x is NaN or y is NaN, return NaN.
        if (double.IsNaN(x) || double.IsNaN(y)) return double.NaN;

        // 2. If x is either +∞𝔽 or -∞𝔽, then
        if (double.IsInfinity(x.Value))
        {
            // a. If y is either +0𝔽 or -0𝔽, return NaN.
            if (y.Value == 0) return double.NaN;

            // b. If y > +0𝔽, return x.
            if (y.Value > 0) return x;

            // c. Return -x.
            return -x.Value;
        }

        // 3. If y is either +∞𝔽 or -∞𝔽, then
        if (double.IsInfinity(y.Value))
        {
            // a. If x is either +0𝔽 or -0𝔽, return NaN.
            if (x.Value == 0) return double.NaN;

            // b. If x > +0𝔽, return y.
            if (x.Value > 0) return y;

            // c. Return -y.
            return -y.Value;
        }

        // FIXME: 4. If x is -0𝔽, then
        // FIXME: a. If y is -0𝔽 or y < -0𝔽, return +0𝔽.
        // FIXME: b. Else, return -0𝔽.
        // FIXME: 5. If y is -0𝔽, then
        // FIXME: a. If x < -0𝔽, return +0𝔽.
        // FIXME: b. Else, return -0𝔽.

        // 6. Return 𝔽(ℝ(x) × ℝ(y)).
        return x.Value * y.Value;
    }

    // 6.1.6.1.5 Number::divide ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-divide
    static internal Number Divide(VM _, Number x, Number y)
    {
        // 1. If x is NaN or y is NaN, return NaN.
        if (double.IsNaN(x) || double.IsNaN(y)) return double.NaN;

        // 2. If x is either +∞𝔽 or -∞𝔽, then
        if (double.IsInfinity(x))
        {
            // a. If y is either +∞𝔽 or -∞𝔽, return NaN.
            if (double.IsInfinity(y.Value)) return double.NaN;

            // FIXME: Handle direct comparisons to +0F, making sure -0F isn't matched
            // b. If y is +0𝔽 or y > +0𝔽, return x.
            if (y.Value >= 0) return x;

            // c. Return -x.
            return -x.Value;
        }

        // 3. If y is +∞𝔽, then
        if (y.Value == double.PositiveInfinity)
        {
            // FIXME: a. If x is +0𝔽 or x > +0𝔽, return +0𝔽. Otherwise, return -0𝔽.
            return 0;
        }

        // 4. If y is -∞𝔽, then
        if (y.Value == double.NegativeInfinity)
        {
            // FIXME: a. If x is +0𝔽 or x > +0𝔽, return -0𝔽. Otherwise, return +0𝔽.
            return 0;
        }

        // FIXME: 5. If x is either + 0𝔽 or -0𝔽, then
        // FIXME: a. If y is either + 0𝔽 or -0𝔽, return NaN.
        // FIXME: b. If y > +0𝔽, return x.
        // FIXME: c. Return -x.
        // FIXME: 6. If y is +0𝔽, then
        // FIXME: a. If x > +0𝔽, return +∞𝔽. Otherwise, return -∞𝔽.
        // FIXME: 7. If y is -0𝔽, then
        // FIXME: a. If x > +0𝔽, return -∞𝔽. Otherwise, return +∞𝔽.

        // 8. Return 𝔽(ℝ(x) / ℝ(y)).
        return x.Value / y.Value;
    }

    // 6.1.6.1.6 Number::remainder ( n, d ), https://tc39.es/ecma262/#sec-numeric-types-number-remainder
    static internal Number Remainder(VM _, Number n, Number d)
    {
        // 1. If n is NaN or d is NaN, return NaN.
        if (double.IsNaN(n) || double.IsNaN(d)) return double.NaN;

        // 2. If n is either +∞𝔽 or -∞𝔽, return NaN.
        if (double.IsInfinity(n.Value)) return double.NaN;

        // 3. If d is either +∞𝔽 or -∞𝔽, return n.
        if (double.IsInfinity(d)) return n;

        // 4. If d is either + 0𝔽 or FIXME: -0𝔽, return NaN.
        if (d == 0) return double.NaN;

        // FIXME: 5. If n is either + 0𝔽 or -0𝔽, return n.
        // FIXME: 6. Assert: n and d are finite and non-zero.
        // FIXME: 7. Let quotient be ℝ(n) / ℝ(d).
        // FIXME: 8. Let q be truncate(quotient).
        // FIXME: 9. Let r be ℝ(n) - (ℝ(d) × q).
        // FIXME: 10. If r = 0 and n< -0𝔽, return -0𝔽.
        // FIXME: 11. Return 𝔽(r).
        return n.Value % d.Value;
    }

    // 6.1.6.1.7 Number::add ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-add
    static internal Number Add(VM _, Number x, Number y)
    {
        // 1. If x is NaN or y is NaN, return NaN.
        if (double.IsNaN(x) || double.IsNaN(y)) return double.NaN;

        // 2. If x is +∞𝔽 and y is -∞𝔽, return NaN.
        if (x.Value == double.PositiveInfinity && y.Value == double.NegativeInfinity) return double.NaN;

        // 3. If x is -∞𝔽 and y is +∞𝔽, return NaN.
        if (x.Value == double.NegativeInfinity && y.Value == double.PositiveInfinity) return double.NaN;

        // 4. If x is either +∞𝔽 or -∞𝔽, return x.
        if (double.IsInfinity(x)) return x;

        // 5. If y is either +∞𝔽 or -∞𝔽, return y.
        if (double.IsInfinity(y)) return y;

        // FIXME: 6. Assert: x and y are both finite.
        // FIXME: 7. If x is -0𝔽 and y is -0𝔽, return -0𝔽.

        // 8. Return 𝔽(ℝ(x) + ℝ(y)).
        return x.Value + y.Value;
    }

    // 6.1.6.1.8 Number::subtract ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-subtract
    static internal Number Subtract(VM vm, Number x, Number y)
    {
        return Add(vm, x, UnaryMinus(y));
    }

    // 6.1.6.1.9 Number::leftShift ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-leftShift
    static internal Number LeftShift(VM vm, Number x, Number y)
    {
        // 1. Let lnum be !ToInt32(x).
        var lnum = MUST(x.ToInt32(vm));

        // 2. Let rnum be !ToUint32(y).
        var rnum = MUST(y.ToUint32(vm));

        // 3. Let shiftCount be ℝ(rnum) modulo 32.
        var shiftCount = rnum % 32;

        // 4. Return the result of left shifting lnum by shiftCount bits.
        // The mathematical value of the result is exactly representable as a 32-bit two's complement bit string.
        return lnum << (int)shiftCount;
    }

    // 6.1.6.1.10 Number::signedRightShift ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-signedRightShift
    static internal Number SignedRightShift(VM vm, Number x, Number y)
    {
        // 1. Let lnum be !ToInt32(x).
        var lnum = MUST(x.ToInt32(vm));

        // 2. Let rnum be !ToUint32(y).
        var rnum = MUST(y.ToUint32(vm));

        // 3. Let shiftCount be ℝ(rnum) modulo 32.
        var shiftCount = rnum % 32;

        // 4. Return the result of performing a sign-extending right shift of lnum by shiftCount bits.
        // The most significant bit is propagated.The mathematical value of the result is exactly representable as a 32-bit two's complement bit string.
        return lnum >> (int)shiftCount;
    }

    // 6.1.6.1.11 Number::unsignedRightShift ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-unsignedRightShift
    static internal Number UnsignedRightShift(VM vm, Number x, Number y)
    {
        // 1. Let lnum be !ToUint32(x).
        var lnum = MUST(x.ToUint32(vm));

        // 2. Let rnum be !ToUint32(y).
        var rnum = MUST(y.ToUint32(vm));

        // 3. Let shiftCount be ℝ(rnum) modulo 32.
        var shiftCount = rnum % 32;

        // 4. Return the result of performing a zero-filling right shift of lnum by shiftCount bits.
        // Vacated bits are filled with zero. The mathematical value of the result is exactly representable as a 32-bit unsigned bit string.
        return lnum >>> (int)shiftCount;
    }

    // 6.1.6.1.12 Number::lessThan(x, y), https://tc39.es/ecma262/#sec-numeric-types-number-lessThan
    static internal Value LessThan(Number x, Number y)
    {
        // 1. If x is NaN, return undefined.
        if (double.IsNaN(x)) return Undefined.The;

        // 2. If y is NaN, return undefined.
        if (double.IsNaN(y)) return Undefined.The;

        // FIXME: 3. If x is y, return false.
        // FIXME: 4. If x is +0𝔽 and y is -0𝔽, return false.
        // FIXME: 5. If x is -0𝔽 and y is +0𝔽, return false.

        // 6. If x is +∞𝔽, return false.
        if (x.Value == double.PositiveInfinity) return false;

        // 7. If y is +∞𝔽, return true.
        if (y.Value == double.PositiveInfinity) return true;

        // 8. If y is -∞𝔽, return false.
        if (y.Value == double.NegativeInfinity) return false;

        // 9. If x is -∞𝔽, return true.
        if (x.Value == double.NegativeInfinity) return true;

        // FIXME: 10. Assert: x and y are finite.

        // 11. If ℝ(x) < ℝ(y), return true. Otherwise, return false.
        return x.Value < y.Value;
    }

    // 6.1.6.1.13 Number::equal ( x, y )
    static internal Boolean Equal(Number x, Number y)
    {
        // 1. If x is NaN, return false.
        if (double.IsNaN(x.Value))
        {
            return false;
        }

        // 2. If y is NaN, return false.
        if (double.IsNaN(y.Value))
        {
            return false;
        }

        // 3. If x is y, return true.
        if (x.Value == y.Value)
        {
            return true;
        }

        // FIXME: 4. If x is +0𝔽 and y is -0𝔽, return true.
        // FIXME: 5. If x is -0𝔽 and y is +0𝔽, return true.

        // 6. Return false.
        return false;
    }

    internal enum BitwiseOp
    {
        AND,
        XOR,
        OR,
    }

    // 6.1.6.1.16 NumberBitwiseOp ( op, x, y ), https://tc39.es/ecma262/#sec-numberbitwiseop
    static internal Number NumberBitwiseOp(VM vm, BitwiseOp op, Number x, Number y)
    {
        // 1. Let lnum be !ToInt32(x).
        var lnum = MUST(x.ToInt32(vm));

        // 2. Let rnum be !ToInt32(y).
        var rnum = MUST(y.ToInt32(vm));

        // 3. Let lbits be the 32-bit two's complement bit string representing ℝ(lnum).
        // 4. Let rbits be the 32-bit two's complement bit string representing ℝ(rnum).

        // 5. If op is &, then
        // a. Let result be the result of applying the bitwise AND operation to lbits and rbits.
        // 6. Else if op is ^, then
        // a. Let result be the result of applying the bitwise exclusive OR(XOR) operation to lbits and rbits.
        // 7. Else,
        // a. Assert: op is |.
        // b. Let result be the result of applying the bitwise inclusive OR operation to lbits and rbits.
        var result = op switch
        {
            BitwiseOp.AND => lnum & rnum,
            BitwiseOp.XOR => lnum ^ rnum,
            BitwiseOp.OR => lnum | rnum,
            _ => throw new InvalidOperationException(),
        };

        // 8. Return the Number value for the integer represented by the 32-bit two's complement bit string result.
        return result;
    }

    // 6.1.6.1.17 Number::bitwiseAND ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseAND
    static internal Number BitwiseAND(VM vm, Number x, Number y)
    {
        // 1. Return NumberBitwiseOp(&, x, y).
        return NumberBitwiseOp(vm, BitwiseOp.AND, x, y);
    }

    // 6.1.6.1.18 Number::bitwiseXOR ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseXOR
    static internal Number BitwiseXOR(VM vm, Number x, Number y)
    {
        // 1. Return NumberBitwiseOp(^, x, y).
        return NumberBitwiseOp(vm, BitwiseOp.XOR, x, y);
    }

    // 6.1.6.1.19 Number::bitwiseOR ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseOR
    static internal Number BitwiseOR(VM vm, Number x, Number y)
    {
        // 1. Return NumberBitwiseOp(|, x, y).
        return NumberBitwiseOp(vm, BitwiseOp.OR, x, y);
    }

    static internal Number NaN
    {
        get
        {
            return _nan;
        }
    }
    static readonly private Number _nan = double.NaN;

    static internal Number Infinity
    {
        get
        {
            return _infinity;
        }
    }
    static readonly private Number _infinity = double.PositiveInfinity;

    public double Value { get; }
}
