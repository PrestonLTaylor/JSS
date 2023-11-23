namespace JSS.Lib.AST;

// 13.5.1 The delete Operator, https://tc39.es/ecma262/#sec-delete-operator
internal sealed class DeleteExpression : IExpression
{
    public DeleteExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.1.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-delete-operator-runtime-semantics-evaluation

    public IExpression Expression { get; }
}
