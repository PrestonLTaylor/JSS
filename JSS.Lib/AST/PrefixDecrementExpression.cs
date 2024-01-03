using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.4.5 Prefix Decrement Operator, https://tc39.es/ecma262/#sec-prefix-decrement-operator
internal sealed class PrefixDecrementExpression : IExpression
{
    public PrefixDecrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.4.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-prefix-decrement-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let expr be ? Evaluation of UnaryExpression.
        var expr = Expression.Evaluate(vm);
        if (expr.IsAbruptCompletion()) return expr;

        // 2. Let oldValue be ? ToNumeric(? GetValue(expr)).
        var getResult = expr.Value.GetValue();
        if (getResult.IsAbruptCompletion()) return getResult;

        var oldValue = getResult.Value.ToNumeric();
        if (oldValue.IsAbruptCompletion()) return oldValue;

        // 3. If oldValue is a Number, then
        Value newValue;
        if (oldValue.Value.IsNumber())
        {
            // a. Let newValue be Number::subtract(oldValue, 1𝔽).
            var asNumber = oldValue.Value.AsNumber();
            newValue = Number.Subtract(asNumber, new Number(1));
        }
        // 4. Else,
        else
        {
            // a. Assert: oldValue is a BigInt.
            Debug.Assert(oldValue.Value.IsBigInt());

            // FIXME: b. Let newValue be BigInt::subtract(oldValue, 1ℤ).
            throw new NotImplementedException();
        }

        // 5. Perform ? PutValue(expr, newValue).
        var putResult = expr.Value.PutValue(vm, newValue);
        if (putResult.IsAbruptCompletion()) return putResult;

        // 6. Return newValue.
        return Completion.NormalCompletion(newValue);
    }

    public IExpression Expression { get; }
}
