namespace JSS.Lib.AST;

// 13.15 Assignment Operators, https://tc39.es/ecma262/#sec-assignment-operators
internal sealed class BinaryOpAssignmentExpression : IExpression
{
    public BinaryOpAssignmentExpression(IExpression lhs, BinaryOpType op, IExpression rhs)
    {
        Lhs = lhs;
        Op = op;
        Rhs = rhs;
    }

    // FIXME: 13.15.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-assignment-operators-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public BinaryOpType Op { get; }
    public IExpression Rhs { get; }
}
