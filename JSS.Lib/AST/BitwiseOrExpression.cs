namespace JSS.Lib.AST;

// BitwiseORExpression, https://tc39.es/ecma262/#prod-BitwiseORExpression
internal sealed class BitwiseOrExpression : IExpression
{
    public BitwiseOrExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.12.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-bitwise-operators-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
