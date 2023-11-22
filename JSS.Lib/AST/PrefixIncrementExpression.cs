namespace JSS.Lib.AST;

// 13.4.4 Prefix Increment Operator, https://tc39.es/ecma262/#sec-prefix-increment-operator
internal sealed class PrefixIncrementExpression : IExpression
{
    public PrefixIncrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.4.4.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-prefix-increment-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
