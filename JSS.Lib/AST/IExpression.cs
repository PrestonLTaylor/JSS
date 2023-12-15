using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using String = JSS.Lib.AST.Values.String;

namespace JSS.Lib.AST;

internal enum BinaryOpType
{
    Exponentiate,
    Multiply,
    Divide,
    Remainder,
    Add,
    Subtract,
    LeftShift,
    SignedRightShift,
    UnsignedRightShift,
    BitwiseAND,
    BitwiseXOR,
    BitwiseOR,
}

internal abstract class IExpression : INode
{
    // 13.15.3 ApplyStringOrNumericBinaryOperator ( lval, opText, rval ), https://tc39.es/ecma262/#sec-applystringornumericbinaryoperator
    static public Completion ApplyStringOrNumericBinaryOperator(Value lval, BinaryOpType op, Value rval)
    {
        // 1. If opText is +, then
        if (op == BinaryOpType.Add)
        {
            // a. Let lprim be ? ToPrimitive(lval).
            var lprim = lval.ToPrimitive();
            if (lprim.IsAbruptCompletion()) return lprim;

            // b. Let rprim be ? ToPrimitive(rval).
            var rprim = rval.ToPrimitive();
            if (rprim.IsAbruptCompletion()) return rprim;

            // c. If lprim is a String or rprim is a String, then
            if (lprim.Value.IsString() || rprim.Value.IsString())
            {
                // i. Let lstr be ? ToString(lprim).
                var lstr = lprim.Value.ToStringJS();
                if (lstr.IsAbruptCompletion()) return lstr;

                // ii. Let rstr be ? ToString(rprim).
                var rstr = rprim.Value.ToStringJS();
                if (rstr.IsAbruptCompletion()) return rstr;

                // iii. Return the string-concatenation of lstr and rstr.
                var lstrValue = lstr.Value as String;
                var rstrValue = rstr.Value as String;
                var concatenation = lstrValue!.Value + rstrValue!.Value;
                return Completion.NormalCompletion(new String(concatenation));
            }

            // d. Set lval to lprim.
            lval = lprim.Value;

            // e. Set rval to rprim.
            rval = rprim.Value;
        }

        // 2. NOTE: At this point, it must be a numeric operation.

        // 3. Let lnum be ? ToNumeric(lval).
        var lnum = lval.ToNumeric();
        if (lnum.IsAbruptCompletion()) return lnum;

        // 4. Let rnum be ? ToNumeric(rval).
        var rnum = rval.ToNumeric();
        if (rnum.IsAbruptCompletion()) return rnum;

        // NOTE: This only happens when one side is a BigInt and the other is a Number
        // FIXME: 5. If Type(lnum) is not Type(rnum), throw a TypeError exception.

        // FIXME: 6. If lnum is a BigInt, then
        // FIXME: a. If opText is **, return ? BigInt::exponentiate(lnum, rnum).
        // FIXME: b. If opText is /, return ? BigInt::divide(lnum, rnum).
        // FIXME: c. If opText is %, return ? BigInt::remainder(lnum, rnum).
        // FIXME: d. If opText is >>>, return ? BigInt::unsignedRightShift(lnum, rnum).

        // 7. Let operation be the abstract operation associated with opText and Type(lnum) in the following table:
        Func<Number, Number, Number> operation = op switch
        {
            BinaryOpType.Exponentiate => Number.Exponentiate,
            BinaryOpType.Multiply => Number.Multiply,
            BinaryOpType.Divide => Number.Divide,
            BinaryOpType.Remainder => Number.Remainder,
            BinaryOpType.Add => Number.Add,
            BinaryOpType.Subtract => Number.Subtract,
            BinaryOpType.LeftShift => Number.LeftShift,
            BinaryOpType.SignedRightShift => Number.SignedRightShift,
            BinaryOpType.UnsignedRightShift => Number.UnsignedRightShift,
            BinaryOpType.BitwiseAND => Number.BitwiseAND,
            BinaryOpType.BitwiseXOR => Number.BitwiseXOR,
            BinaryOpType.BitwiseOR => Number.BitwiseOR,
            _ => throw new InvalidOperationException(),
        };

        var result = operation((lnum.Value as Number)!, (rnum.Value as Number)!);
        return Completion.NormalCompletion(result);
    }

    // 13.15.4 EvaluateStringOrNumericBinaryExpression ( leftOperand, opText, rightOperand )
    static public Completion EvaluateStringOrNumericBinaryExpression(VM vm, IExpression lhs, BinaryOpType op, IExpression rhs)
    {
        // 1.Let lref be ? Evaluation of leftOperand.
        var lref = lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue();
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of rightOperand.
        var rref = rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue();
        if (rval.IsAbruptCompletion()) return rval;

        // 5. Return ? ApplyStringOrNumericBinaryOperator(lval, opText, rval).
        return ApplyStringOrNumericBinaryOperator(lval.Value, op, rval.Value);
    }
}
