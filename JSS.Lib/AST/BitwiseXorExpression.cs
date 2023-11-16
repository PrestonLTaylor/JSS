namespace JSS.Lib.AST;

// BitwiseXORExpression, https://tc39.es/ecma262/#prod-BitwiseXORExpression
internal sealed class BitwiseXorExpression : IExpression
{
    public BitwiseXorExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.12.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-bitwise-operators-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
