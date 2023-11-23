namespace JSS.Lib.AST;

// LogicalORExpression, https://tc39.es/ecma262/#prod-LogicalORExpression
internal sealed class LogicalOrExpression : IExpression
{
    public LogicalOrExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.13.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-logical-operators-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
