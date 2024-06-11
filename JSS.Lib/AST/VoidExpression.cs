using JSS.Lib.Execution;
using JSS.Lib.AST.Values;

namespace JSS.Lib.AST;

// 13.5.2 The void Operator, https://tc39.es/ecma262/#sec-void-operator
internal sealed class VoidExpression : IExpression
{
    public VoidExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-void-operator-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let expr be ? Evaluation of UnaryExpression.
        var expr = Expression.Evaluate(vm);
        if (expr.IsAbruptCompletion()) return expr;

        // 2. Perform ? GetValue(expr).
        var value = expr.Value.GetValue(vm);
        if (value.IsAbruptCompletion()) return value;

        // 3. Return undefined.
        return Undefined.The;
    }

    public IExpression Expression { get; }
}
