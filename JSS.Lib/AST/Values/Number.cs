namespace JSS.Lib.AST.Values;

internal sealed class Number : Value
{
    public Number(double value)
    {
        Value = value;
    }

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
    static public Number UnaryMinus(Number x)
    {
        // 1. If x is NaN, return NaN.
        if (double.IsNaN(x.Value)) return x;

        // 2. Return the result of negating x; that is, compute a Number with the same magnitude but opposite sign.
        return new Number(-x.Value);
    }

    // 6.1.6.1.2 Number::bitwiseNOT ( x ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseNOT
    static public Number BitwiseNOT(Number x)
    {
        // FIXME: 1. Let oldValue be ! ToInt32(x).

        // 2. Return the result of applying bitwise complement to oldValue.
        // The mathematical value of the result is exactly representable as a 32-bit two's complement bit string.
        var result = ~(int)x.Value;
        return new Number(result);
    }

    // 6.1.6.1.3 Number::exponentiate ( base, exponent ), https://tc39.es/ecma262/#sec-numeric-types-number-exponentiate
    static public Number Exponentiate(Number expBase, Number exponent)
    {
        // FIXME: 1. If exponent is NaN, return NaN.
        // FIXME: 2. If exponent is either + 0𝔽 or - 0𝔽, return 1𝔽.
        // FIXME: 3. If base is NaN, return NaN.
        // FIXME: 4. If base is +∞𝔽, then
        // FIXME: a. If exponent > +0𝔽, return +∞𝔽. Otherwise, return +0𝔽.
        // FIXME: 5. If base is -∞𝔽, then
        // FIXME: a. If exponent > +0𝔽, then
        // FIXME: i. If exponent is an odd integral Number, return -∞𝔽. Otherwise, return +∞𝔽.
        // FIXME: b. Else,
        // FIXME: i. If exponent is an odd integral Number, return -0𝔽. Otherwise, return +0𝔽.
        // FIXME: 6. If base is +0𝔽, then
        // FIXME: a. If exponent > +0𝔽, return +0𝔽. Otherwise, return +∞𝔽.
        // FIXME: 7. If base is -0𝔽, then
        // FIXME: a. If exponent > +0𝔽, then
        // FIXME: i. If exponent is an odd integral Number, return -0𝔽. Otherwise, return +0𝔽.
        // FIXME: b. Else,
        // FIXME: i. If exponent is an odd integral Number, return -∞𝔽. Otherwise, return +∞𝔽.
        // FIXME: 8. Assert: base is finite and is neither + 0𝔽 nor - 0𝔽.
        // FIXME: 9. If exponent is +∞𝔽, then
        // FIXME: a. If abs(ℝ(base)) > 1, return +∞𝔽.
        // FIXME: b. If abs(ℝ(base)) = 1, return NaN.
        // FIXME: c. If abs(ℝ(base)) < 1, return +0𝔽.
        // FIXME: 10. If exponent is -∞𝔽, then
        // FIXME: a. If abs(ℝ(base)) > 1, return +0𝔽.
        // FIXME: b. If abs(ℝ(base)) = 1, return NaN.
        // FIXME: c. If abs(ℝ(base)) < 1, return +∞𝔽.
        // FIXME: 11. Assert: exponent is finite and is neither + 0𝔽 nor - 0𝔽.
        // FIXME: 12. If base < -0𝔽 and exponent is not an integral Number, return NaN.

        // 13. Return an implementation-approximated Number value representing the result of raising ℝ(base) to the ℝ(exponent) power.
        var result = Math.Pow(expBase.Value, exponent.Value);
        return new Number(result);
    }

    // 6.1.6.1.4 Number::multiply ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-multiply
    static public Number Multiply(Number x, Number y)
    {
        // FIXME: 1. If x is NaN or y is NaN, return NaN.
        // FIXME: 2. If x is either +∞𝔽 or -∞𝔽, then
        // FIXME: a. If y is either + 0𝔽 or -0𝔽, return NaN.
        // FIXME: b. If y > +0𝔽, return x.
        // FIXME: c. Return -x.
        // FIXME: 3. If y is either +∞𝔽 or -∞𝔽, then
        // FIXME: a. If x is either +0𝔽 or -0𝔽, return NaN.
        // FIXME: b. If x > +0𝔽, return y.
        // FIXME: c. Return -y.
        // FIXME: 4. If x is -0𝔽, then
        // FIXME: a. If y is -0𝔽 or y < -0𝔽, return +0𝔽.
        // FIXME: b. Else, return -0𝔽.
        // FIXME: 5. If y is -0𝔽, then
        // FIXME: a. If x < -0𝔽, return +0𝔽.
        // FIXME: b. Else, return -0𝔽.

        // 6. Return 𝔽(ℝ(x) × ℝ(y)).
        var result = x.Value * y.Value;
        return new Number(result);
    }

    // 6.1.6.1.5 Number::divide ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-divide
    static public Number Divide(Number x, Number y)
    {
        // FIXME: 1. If x is NaN or y is NaN, return NaN.
        // FIXME: 2. If x is either +∞𝔽 or -∞𝔽, then
        // FIXME: a. If y is either +∞𝔽 or -∞𝔽, return NaN.
        // FIXME: b. If y is +0𝔽 or y > +0𝔽, return x.
        // FIXME: c. Return -x.
        // FIXME: 3. If y is +∞𝔽, then
        // FIXME: a. If x is +0𝔽 or x > +0𝔽, return +0𝔽. Otherwise, return -0𝔽.
        // FIXME: 4. If y is -∞𝔽, then
        // FIXME: a. If x is +0𝔽 or x > +0𝔽, return -0𝔽. Otherwise, return +0𝔽.
        // FIXME: 5. If x is either + 0𝔽 or -0𝔽, then
        // FIXME: a. If y is either + 0𝔽 or -0𝔽, return NaN.
        // FIXME: b. If y > +0𝔽, return x.
        // FIXME: c. Return -x.
        // FIXME: 6. If y is +0𝔽, then
        // FIXME: a. If x > +0𝔽, return +∞𝔽. Otherwise, return -∞𝔽.
        // FIXME: 7. If y is -0𝔽, then
        // FIXME: a. If x > +0𝔽, return -∞𝔽. Otherwise, return +∞𝔽.

        // 8. Return 𝔽(ℝ(x) / ℝ(y)).
        var result = x.Value / y.Value;
        return new Number(result);
    }

    // 6.1.6.1.6 Number::remainder ( n, d ), https://tc39.es/ecma262/#sec-numeric-types-number-remainder
    static public Number Remainder(Number n, Number d)
    {
        // FIXME: 1. If n is NaN or d is NaN, return NaN.
        // FIXME: 2. If n is either +∞𝔽 or -∞𝔽, return NaN.
        // FIXME: 3. If d is either +∞𝔽 or -∞𝔽, return n.
        // FIXME: 4. If d is either + 0𝔽 or -0𝔽, return NaN.
        // FIXME: 5. If n is either + 0𝔽 or -0𝔽, return n.
        // FIXME: 6. Assert: n and d are finite and non-zero.
        // FIXME: 7. Let quotient be ℝ(n) / ℝ(d).
        // FIXME: 8. Let q be truncate(quotient).
        // FIXME: 9. Let r be ℝ(n) - (ℝ(d) × q).
        // FIXME: 10. If r = 0 and n< -0𝔽, return -0𝔽.
        // FIXME: 11. Return 𝔽(r).
        var result = n.Value % d.Value;
        return new Number(result);
    }

    // 6.1.6.1.7 Number::add ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-add
    static public Number Add(Number x, Number y)
    {
        // FIXME: 1. If x is NaN or y is NaN, return NaN.
        // FIXME: 2. If x is +∞𝔽 and y is -∞𝔽, return NaN.
        // FIXME: 3. If x is -∞𝔽 and y is +∞𝔽, return NaN.
        // FIXME: 4. If x is either +∞𝔽 or -∞𝔽, return x.
        // FIXME: 5. If y is either +∞𝔽 or -∞𝔽, return y.
        // FIXME: 6. Assert: x and y are both finite.
        // FIXME: 7. If x is -0𝔽 and y is -0𝔽, return -0𝔽.

        // 8. Return 𝔽(ℝ(x) + ℝ(y)).
        var result = x.Value + y.Value;
        return new Number(result);
    }

    // 6.1.6.1.8 Number::subtract ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-subtract
    static public Number Subtract(Number x, Number y)
    {
        return Add(x, UnaryMinus(y));
    }

    // 6.1.6.1.9 Number::leftShift ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-leftShift
    static public Number LeftShift(Number x, Number y)
    {
        // FIXME: 1. Let lnum be !ToInt32(x).
        // FIXME: 2. Let rnum be !ToUint32(y).
        // FIXME: 3. Let shiftCount be ℝ(rnum) modulo 32.
        // FIXME: 4. Return the result of left shifting lnum by shiftCount bits.
        // The mathematical value of the result is exactly representable as a 32-bit two's complement bit string.

        var result = (int)x.Value << (int)y.Value;
        return new Number(result);
    }

    // 6.1.6.1.10 Number::signedRightShift ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-signedRightShift
    static public Number SignedRightShift(Number x, Number y)
    {
        // FIXME: 1. Let lnum be !ToInt32(x).
        // FIXME: 2. Let rnum be !ToUint32(y).
        // FIXME: 3. Let shiftCount be ℝ(rnum) modulo 32.
        // FIXME: 4. Return the result of performing a sign-extending right shift of lnum by shiftCount bits.
        // The most significant bit is propagated.The mathematical value of the result is exactly representable as a 32-bit two's complement bit string.

        var result = (int)x.Value >> (int)y.Value;
        return new Number(result);
    }

    // 6.1.6.1.11 Number::unsignedRightShift ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-unsignedRightShift
    static public Number UnsignedRightShift(Number x, Number y)
    {
        // FIXME: 1. Let lnum be !ToUint32(x).
        // FIXME: 2. Let rnum be !ToUint32(y).
        // FIXME: 3. Let shiftCount be ℝ(rnum) modulo 32.
        // FIXME: 4. Return the result of performing a zero-filling right shift of lnum by shiftCount bits.
        // Vacated bits are filled with zero. The mathematical value of the result is exactly representable as a 32-bit unsigned bit string.

        var result = (int)x.Value >>> (int)y.Value;
        return new Number(result);
    }

    // 6.1.6.1.12 Number::lessThan(x, y), https://tc39.es/ecma262/#sec-numeric-types-number-lessThan
    static public Value LessThan(Number x, Number y)
    {
        // FIXME: 1. If x is NaN, return undefined.
        // FIXME: 2. If y is NaN, return undefined.
        // FIXME: 3. If x is y, return false.
        // FIXME: 4. If x is +0𝔽 and y is -0𝔽, return false.
        // FIXME: 5. If x is -0𝔽 and y is +0𝔽, return false.
        // FIXME: 6. If x is +∞𝔽, return false.
        // FIXME: 7. If y is +∞𝔽, return true.
        // FIXME: 8. If y is -∞𝔽, return false.
        // FIXME: 9. If x is -∞𝔽, return true.
        // FIXME: 10. Assert: x and y are finite.

        // 11. If ℝ(x) < ℝ(y), return true. Otherwise, return false.
        return new Boolean(x.Value < y.Value);
    }

    // 6.1.6.1.13 Number::equal ( x, y )
    static public Boolean Equal(Number x, Number y)
    {
        // 1. If x is NaN, return false.
        if (double.IsNaN(x.Value))
        {
            return new Boolean(false);
        }

        // 2. If y is NaN, return false.
        if (double.IsNaN(y.Value))
        {
            return new Boolean(false);
        }

        // 3. If x is y, return true.
        if (x.Value == y.Value)
        {
            return new Boolean(true);
        }

        // FIXME: 4. If x is +0𝔽 and y is -0𝔽, return true.
        // FIXME: 5. If x is -0𝔽 and y is +0𝔽, return true.

        // 6. Return false.
        return new Boolean(false);
    }

    internal enum BitwiseOp
    {
        AND,
        XOR,
        OR,
    }

    // 6.1.6.1.16 NumberBitwiseOp ( op, x, y ), https://tc39.es/ecma262/#sec-numberbitwiseop
    static public Number NumberBitwiseOp(BitwiseOp op, Number x, Number y)
    {
        // FIXME: 1. Let lnum be !ToInt32(x).
        // FIXME: 2. Let rnum be !ToInt32(y).
        // FIXME: 3. Let lbits be the 32-bit two's complement bit string representing ℝ(lnum).
        // FIXME: 4. Let rbits be the 32-bit two's complement bit string representing ℝ(rnum).

        // 5. If op is &, then
        // FIXME: a. Let result be the result of applying the bitwise AND operation to lbits and rbits.
        // 6. Else if op is ^, then
        // FIXME: a. Let result be the result of applying the bitwise exclusive OR(XOR) operation to lbits and rbits.
        // 7. Else,
        // a. Assert: op is |.
        // FIXME: b. Let result be the result of applying the bitwise inclusive OR operation to lbits and rbits.
        var result = op switch
        {
            BitwiseOp.AND => (int)x.Value & (int)y.Value,
            BitwiseOp.XOR => (int)x.Value ^ (int)y.Value,
            BitwiseOp.OR => (int)x.Value | (int)y.Value,
            _ => throw new InvalidOperationException(),
        };

        // FIXME: 8. Return the Number value for the integer represented by the 32-bit two's complement bit string result.
        return new Number(result);
    }

    // 6.1.6.1.17 Number::bitwiseAND ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseAND
    static public Number BitwiseAND(Number x, Number y)
    {
        // 1. Return NumberBitwiseOp(&, x, y).
        return NumberBitwiseOp(BitwiseOp.AND, x, y);
    }

    // 6.1.6.1.18 Number::bitwiseXOR ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseXOR
    static public Number BitwiseXOR(Number x, Number y)
    {
        // 1. Return NumberBitwiseOp(^, x, y).
        return NumberBitwiseOp(BitwiseOp.XOR, x, y);
    }

    // 6.1.6.1.19 Number::bitwiseOR ( x, y ), https://tc39.es/ecma262/#sec-numeric-types-number-bitwiseOR
    static public Number BitwiseOR(Number x, Number y)
    {
        // 1. Return NumberBitwiseOp(|, x, y).
        return NumberBitwiseOp(BitwiseOp.OR, x, y);
    }

    static public Number NaN
    {
        get
        {
            return _nan;
        }
    }
    static readonly private Number _nan = new(double.NaN);

    public double Value { get; }
}
