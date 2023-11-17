namespace JSS.Lib.AST;

// 13.8.1 The Addition Operator ( + ), https://tc39.es/ecma262/#sec-addition-operator-plus
internal sealed class AdditionExpression : IExpression
{
    public AdditionExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.8.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-addition-operator-plus-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
