using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.9.2 The Signed Right Shift Operator ( >> ), https://tc39.es/ecma262/#sec-signed-right-shift-operator
internal sealed class RightShiftExpression : IExpression
{
    public RightShiftExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.9.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-signed-right-shift-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return ? EvaluateStringOrNumericBinaryExpression(ShiftExpression, >>, AdditiveExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.SignedRightShift, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
