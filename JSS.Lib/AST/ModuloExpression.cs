namespace JSS.Lib.AST;

// 13.7 Multiplicative Operators, https://tc39.es/ecma262/#sec-multiplicative-operators
internal sealed class ModuloExpression : IExpression
{
    public ModuloExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-multiplicative-operators-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
