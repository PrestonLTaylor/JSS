using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;

namespace JSS.Lib.AST;

// 13.5.7 Logical NOT Operator ( ! ), https://tc39.es/ecma262/#sec-logical-not-operator
internal sealed class LogicalNotExpression : IExpression
{
    public LogicalNotExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-logical-not-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let expr be ? Evaluation of UnaryExpression.
        var expr = Expression.Evaluate(vm);
        if (expr.IsAbruptCompletion()) return expr;

        // 2. Let oldValue be ToBoolean(? GetValue(expr)).
        var value = expr.Value.GetValue();
        if (value.IsAbruptCompletion()) return value;

        var oldValue = value.Value.ToBoolean();

        // 3. If oldValue is true, return false.
        // 4. Return true.
        return new Boolean(!oldValue.Value);
    }

    public IExpression Expression { get; }
}
