using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.7 Multiplicative Operators, https://tc39.es/ecma262/#sec-multiplicative-operators
internal sealed class DivisionExpression : IExpression
{
    public DivisionExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-multiplicative-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let opText be the source text matched by MultiplicativeOperator.
        // 2. Return ? EvaluateStringOrNumericBinaryExpression(MultiplicativeExpression, opText, ExponentiationExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.Divide, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
