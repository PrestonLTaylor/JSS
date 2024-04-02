using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.5.4 Unary + Operator, https://tc39.es/ecma262/#sec-unary-plus-operator
internal sealed class UnaryPlusExpression : IExpression
{
    public UnaryPlusExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.4.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-unary-plus-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let expr be ? Evaluation of UnaryExpression.
        var expr = Expression.Evaluate(vm);
        if (expr.IsAbruptCompletion()) return expr;

        // 2. Return ? ToNumber(? GetValue(expr)).
        var exprValue = expr.Value.GetValue();
        if (exprValue.IsAbruptCompletion()) return exprValue;

        return exprValue.Value.ToNumber();
    }

    public IExpression Expression { get; }
}
