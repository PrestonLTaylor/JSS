using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.8.2 The Subtraction Operator ( - ), https://tc39.es/ecma262/#sec-subtraction-operator-minus
internal sealed class SubtractionExpression : IExpression
{
    public SubtractionExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.8.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-subtraction-operator-minus-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return ? EvaluateStringOrNumericBinaryExpression(AdditiveExpression, -, MultiplicativeExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.Subtract, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
