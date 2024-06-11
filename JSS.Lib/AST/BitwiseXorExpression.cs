using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// BitwiseXORExpression, https://tc39.es/ecma262/#prod-BitwiseXORExpression
internal sealed class BitwiseXorExpression : IExpression
{
    public BitwiseXorExpression(IExpression lhs, IExpression rhs)
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

        // 1. Return ? EvaluateStringOrNumericBinaryExpression(BitwiseXORExpression, ^, BitwiseANDExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.BitwiseXOR, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
