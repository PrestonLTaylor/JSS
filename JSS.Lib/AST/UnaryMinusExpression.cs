using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.5.5 Unary - Operator, https://tc39.es/ecma262/#sec-unary-minus-operator
internal sealed class UnaryMinusExpression : IExpression
{
    public UnaryMinusExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-unary-minus-operator-runtime-semantics-evaluation
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
            // a. Return Number::unaryMinus(oldValue).
            var asNumber = oldValue.Value.AsNumber();
            return Number.UnaryMinus(asNumber);
        }
        // 4. Else,
        else
        {
            // a. Assert: oldValue is a BigInt.
            Debug.Assert(oldValue.Value.IsBigInt());

            // FIXME: b. Return BigInt::unaryMinus(oldValue).
            throw new NotImplementedException();
        }
    }

    public IExpression Expression { get; }
}
