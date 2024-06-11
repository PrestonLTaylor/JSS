using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.5 Expression Statement, https://tc39.es/ecma262/#sec-expression-statement
internal sealed class ExpressionStatement : INode
{
    public ExpressionStatement(IExpression expression)
    {
        Expression = expression;
    }

    // 14.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-expression-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let exprRef be ? Evaluation of Expression.
        var exprRef = Expression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Return ? GetValue(exprRef).
        return exprRef.Value.GetValue(vm);
    }

    public IExpression Expression { get; }
}
