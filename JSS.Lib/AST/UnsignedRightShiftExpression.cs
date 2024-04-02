using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.9.3 The Unsigned Right Shift Operator ( >>> ), https://tc39.es/ecma262/#sec-unsigned-right-shift-operator
internal sealed class UnsignedRightShiftExpression : IExpression
{
    public UnsignedRightShiftExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.9.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-signed-right-shift-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? EvaluateStringOrNumericBinaryExpression(ShiftExpression, >>>, AdditiveExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.UnsignedRightShift, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
