namespace JSS.Lib.AST;

// ComputedPropertyName, https://tc39.es/ecma262/#prod-ComputedPropertyName
internal sealed class ComputedPropertyName : INode
{
    public ComputedPropertyName(IExpression expression)
    {
        Expression = expression;
    }

    public IExpression Expression { get; }
}
