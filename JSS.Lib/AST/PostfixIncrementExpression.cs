namespace JSS.Lib.AST;

// 13.4.2 Postfix Increment Operator, https://tc39.es/ecma262/#sec-postfix-increment-operator
internal sealed class PostfixIncrementExpression : IExpression
{
    public PostfixIncrementExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.4.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-postfix-increment-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
