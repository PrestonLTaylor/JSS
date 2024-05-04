namespace JSS.Lib.AST;

// LiteralPropertyName, https://tc39.es/ecma262/#prod-LiteralPropertyName
internal sealed class LiteralPropertyName : INode
{
    public LiteralPropertyName(INode literal)
    {
        Literal = literal;
    }

    public INode Literal { get; }
}
