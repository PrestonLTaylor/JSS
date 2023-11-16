namespace JSS.Lib.AST;

// LogicalANDExpression, https://tc39.es/ecma262/#prod-LogicalANDExpression
internal sealed class LogicalAndExpression : IExpression
{
    public LogicalAndExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.13.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-logical-operators-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
