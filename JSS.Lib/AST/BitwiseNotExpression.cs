namespace JSS.Lib.AST;

// 13.5.6 Bitwise NOT Operator ( ~ ), https://tc39.es/ecma262/#sec-bitwise-not-operator
internal sealed class BitwiseNotExpression : IExpression
{
    public BitwiseNotExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.6.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-bitwise-not-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
