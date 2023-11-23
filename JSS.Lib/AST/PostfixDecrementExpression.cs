namespace JSS.Lib.AST;

// 13.4.3 Postfix Decrement Operator, https://tc39.es/ecma262/#sec-postfix-decrement-operator
internal sealed class PostfixDecrementExpression : IExpression
{
    public PostfixDecrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.4.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-postfix-decrement-operator-runtime-semantics-evaluation

    public IExpression Expression { get; }
}
