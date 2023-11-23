namespace JSS.Lib.AST;

// 13.5.2 The void Operator, https://tc39.es/ecma262/#sec-void-operator
internal sealed class VoidExpression : IExpression
{
    public VoidExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-void-operator-runtime-semantics-evaluation

    public IExpression Expression { get; }
}
