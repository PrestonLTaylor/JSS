namespace JSS.Lib.AST;

// 13.9.2 The Signed Right Shift Operator ( >> ), https://tc39.es/ecma262/#sec-signed-right-shift-operator
internal sealed class RightShiftExpression : IExpression
{
    public RightShiftExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.9.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-signed-right-shift-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
