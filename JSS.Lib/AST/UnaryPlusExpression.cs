namespace JSS.Lib.AST;

// 13.5.4 Unary + Operator, https://tc39.es/ecma262/#sec-unary-plus-operator
internal sealed class UnaryPlusExpression : IExpression
{
    public UnaryPlusExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.4.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-unary-plus-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
