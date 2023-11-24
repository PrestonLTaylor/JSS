using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// BitwiseANDExpression, https://tc39.es/ecma262/#prod-BitwiseANDExpression
internal sealed class BitwiseAndExpression : IExpression
{
    public BitwiseAndExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.12.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-bitwise-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? EvaluateStringOrNumericBinaryExpression(BitwiseANDExpression, &, EqualityExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.BitwiseAND, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
