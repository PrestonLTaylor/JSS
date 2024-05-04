namespace JSS.Lib.AST;

internal sealed class PropertyNameDefinition : INode
{
    public PropertyNameDefinition(INode propertyName, IExpression expression)
    {
        PropertyName = propertyName;
        Expression = expression;
    }

    public INode PropertyName { get; }
    public IExpression Expression { get; }
}
