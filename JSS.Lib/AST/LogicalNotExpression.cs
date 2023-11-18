namespace JSS.Lib.AST;

// 13.5.7 Logical NOT Operator ( ! ), https://tc39.es/ecma262/#sec-logical-not-operator
internal sealed class LogicalNotExpression : IExpression
{
    public LogicalNotExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-logical-not-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
