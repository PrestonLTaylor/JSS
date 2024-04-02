using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.8.1 The Addition Operator ( + ), https://tc39.es/ecma262/#sec-addition-operator-plus
internal sealed class AdditionExpression : IExpression
{
    public AdditionExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.8.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-addition-operator-plus-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? EvaluateStringOrNumericBinaryExpression(AdditiveExpression, +, MultiplicativeExpression).
        return EvaluateStringOrNumericBinaryExpression(vm, Lhs, BinaryOpType.Add, Rhs);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
