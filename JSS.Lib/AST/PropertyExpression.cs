namespace JSS.Lib.AST;

// 13.3.2 Property Accessors, https://tc39.es/ecma262/#sec-property-accessors
internal sealed class PropertyExpression : IExpression
{
    public PropertyExpression(IExpression lhs, string rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.3.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-property-accessors-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public string Rhs { get; }
}
