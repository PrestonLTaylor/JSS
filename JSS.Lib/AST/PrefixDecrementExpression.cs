namespace JSS.Lib.AST;

// 13.4.5 Prefix Decrement Operator, https://tc39.es/ecma262/#sec-prefix-decrement-operator
internal sealed class PrefixDecrementExpression : IExpression
{
    public PrefixDecrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.4.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-prefix-decrement-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
