using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.5.6 Bitwise NOT Operator ( ~ ), https://tc39.es/ecma262/#sec-bitwise-not-operator
internal sealed class BitwiseNotExpression : IExpression
{
    public BitwiseNotExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.6.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-bitwise-not-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let expr be ? Evaluation of UnaryExpression.
        var expr = Expression.Evaluate(vm);
        if (expr.IsAbruptCompletion()) return expr;

        // 2. Let oldValue be ? ToNumeric(? GetValue(expr)).
        var exprValue = expr.Value.GetValue(vm);
        if (exprValue.IsAbruptCompletion()) return exprValue;

        var oldValue = exprValue.Value.ToNumeric(vm);
        if (oldValue.IsAbruptCompletion()) return oldValue;

        // 3. If oldValue is a Number, then
        if (oldValue.Value.IsNumber())
        {
            // a. Return Number::bitwiseNOT(oldValue).
            var asNumber = oldValue.Value.AsNumber();
            return Number.BitwiseNOT(asNumber);
        }
        // 4. Else,
        else
        {
            // a. Assert: oldValue is a BigInt.
            Assert(oldValue.Value.IsBigInt(), "a. Assert: oldValue is a BigInt.");

            // FIXME: b. Return BigInt::bitwiseNOT(oldValue).
            throw new NotImplementedException();
        }
    }

    public IExpression Expression { get; }
}
