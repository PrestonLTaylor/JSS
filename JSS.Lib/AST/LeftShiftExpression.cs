using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.9.1 The Left Shift Operator ( << ), https://tc39.es/ecma262/#sec-left-shift-operator
internal sealed class LeftShiftExpression : IExpression
{
    public LeftShiftExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.9.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-left-shift-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return ? EvaluateStringOrNumericBinaryExpression(ShiftExpression, <<, AdditiveExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.LeftShift, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
