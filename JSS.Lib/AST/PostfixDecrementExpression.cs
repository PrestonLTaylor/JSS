using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.4.3 Postfix Decrement Operator, https://tc39.es/ecma262/#sec-postfix-decrement-operator
internal sealed class PostfixDecrementExpression : IExpression
{
    public PostfixDecrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.4.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-postfix-decrement-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let lhs be ? Evaluation of LeftHandSideExpression.
        var lhs = Expression.Evaluate(vm);
        if (lhs.IsAbruptCompletion()) return lhs;

        // 2. Let oldValue be ? ToNumeric(? GetValue(lhs)).
        var getResult = lhs.Value.GetValue(vm);
        if (getResult.IsAbruptCompletion()) return getResult;

        var oldValue = getResult.Value.ToNumeric();
        if (oldValue.IsAbruptCompletion()) return oldValue;

        // 3. If oldValue is a Number, then
        Value newValue;
        if (oldValue.Value.IsNumber())
        {
            // a. Let newValue be Number::subtract(oldValue, 1𝔽).
            var oldAsNumber = oldValue.Value.AsNumber();
            newValue = Number.Subtract(oldAsNumber, 1);
        }
        // 4. Else,
        else
        {
            // a. Assert: oldValue is a BigInt.
            Debug.Assert(oldValue.Value.IsBigInt());

            // FIXME: b. Let newValue be BigInt::subtract(oldValue, 1ℤ).
            throw new NotImplementedException();
        }

        // 5. Perform ? PutValue(lhs, newValue).
        var putResult = lhs.Value.PutValue(vm, newValue);
        if (putResult.IsAbruptCompletion()) return putResult;

        // 6. Return oldValue.
        return oldValue;
    }

    public IExpression Expression { get; }
}
