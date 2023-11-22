namespace JSS.Lib.AST;

// 13.8.2 The Subtraction Operator ( - ), https://tc39.es/ecma262/#sec-subtraction-operator-minus
internal sealed class SubtractionExpression : IExpression
{
    public SubtractionExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.8.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-subtraction-operator-minus-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
