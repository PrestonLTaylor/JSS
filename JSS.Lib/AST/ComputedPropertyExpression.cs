namespace JSS.Lib.AST;

// 13.3.2 Property Accessors, https://tc39.es/ecma262/#sec-property-accessors
internal sealed class ComputedPropertyExpression : IExpression
{
    public ComputedPropertyExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.3.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-property-accessors-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
