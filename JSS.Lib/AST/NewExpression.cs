namespace JSS.Lib.AST;

// 13.3.5 The new Operator, https://tc39.es/ecma262/#sec-new-operator
internal sealed class NewExpression : IExpression
{
    public NewExpression(IExpression expression, IReadOnlyList<IExpression> arguments)
    {
        Expression = expression;
        Arguments = arguments;
    }

    // FIXME: 13.3.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-new-operator-runtime-semantics-evaluation

    public IExpression Expression { get; }
    public IReadOnlyList<IExpression> Arguments { get; }
}
