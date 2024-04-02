using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.6 Exponentiation Operator, https://tc39.es/ecma262/#sec-exp-operator
internal sealed class ExponentiationExpression : IExpression
{
    public ExponentiationExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.6.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-exp-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? EvaluateStringOrNumericBinaryExpression(UpdateExpression, **, ExponentiationExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.Exponentiate, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
