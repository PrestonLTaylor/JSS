using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// BitwiseORExpression, https://tc39.es/ecma262/#prod-BitwiseORExpression
internal sealed class BitwiseOrExpression : IExpression
{
    public BitwiseOrExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.12.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-bitwise-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return ? EvaluateStringOrNumericBinaryExpression(BitwiseORExpression, |, BitwiseXORExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.BitwiseOR, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
