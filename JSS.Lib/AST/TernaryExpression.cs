namespace JSS.Lib.AST;

// 13.14 Conditional Operator ( ? : ), https://tc39.es/ecma262/#prod-ConditionalExpression
internal sealed class TernaryExpression : IExpression
{
    public TernaryExpression(IExpression testExpression, IExpression trueCaseExpression,  IExpression falseCaseExpression)
    {
        TestExpression = testExpression;
        TrueCaseExpression = trueCaseExpression;
        FalseCaseExpression = falseCaseExpression;
    }

    public IExpression TestExpression { get; }
    public IExpression TrueCaseExpression { get; }
    public IExpression FalseCaseExpression { get; }
}
