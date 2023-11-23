namespace JSS.Lib.AST;

// 13.9.3 The Unsigned Right Shift Operator ( >>> ), https://tc39.es/ecma262/#sec-unsigned-right-shift-operator
internal sealed class UnsignedRightShiftExpression : IExpression
{
    public UnsignedRightShiftExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.9.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-unsigned-right-shift-operator-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
