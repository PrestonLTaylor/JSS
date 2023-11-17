namespace JSS.Lib.AST;

// 13.9.1 The Left Shift Operator ( << ), https://tc39.es/ecma262/#sec-left-shift-operator
internal sealed class LeftShiftExpression : IExpression
{
    public LeftShiftExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.9.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-left-shift-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
