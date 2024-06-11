using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.7 Multiplicative Operators, https://tc39.es/ecma262/#sec-multiplicative-operators
internal sealed class MultiplicationExpression : IExpression
{
    public MultiplicationExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-multiplicative-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let opText be the source text matched by MultiplicativeOperator.
        // 2. Return ? EvaluateStringOrNumericBinaryExpression(MultiplicativeExpression, opText, ExponentiationExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.Multiply, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
