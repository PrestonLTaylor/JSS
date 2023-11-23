namespace JSS.Lib.AST;

// 13.5.5 Unary - Operator, https://tc39.es/ecma262/#sec-unary-minus-operator
internal sealed class UnaryMinusExpression : IExpression
{
    public UnaryMinusExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-unary-minus-operator-runtime-semantics-evaluation

    public IExpression Expression { get; }
}
