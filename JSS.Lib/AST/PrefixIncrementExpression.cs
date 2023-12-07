using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.4.4 Prefix Increment Operator, https://tc39.es/ecma262/#sec-prefix-increment-operator
internal sealed class PrefixIncrementExpression : IExpression
{
    public PrefixIncrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.4.4.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-prefix-increment-operator-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let expr be ? Evaluation of UnaryExpression.
        var expr = Expression.Evaluate(vm);

        // Let oldValue be ? ToNumeric(? GetValue(expr)).
        var getResult = expr.Value.GetValue();
        if (getResult.IsAbruptCompletion()) return getResult;

        var oldValue = getResult.Value.ToNumeric(vm);
        if (oldValue.IsAbruptCompletion()) return oldValue;

        // 3. If oldValue is a Number, then
        Value newValue;
        if (oldValue.Value.IsNumber())
        {
            // a. Let newValue be Number::add(oldValue, 1𝔽).
            var asNumber = (oldValue.Value as Number)!;
            newValue = Number.Add(asNumber, new Number(1));
        }
        // 4. Else,
        else
        {
            // a. Assert: oldValue is a BigInt.
            Debug.Assert(oldValue.Value.IsBigInt());

            // FIXME: b. Let newValue be BigInt::add(oldValue, 1ℤ).
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
