namespace JSS.Lib.AST;

// 14.7.5 The for-in, for-of, and for-await-of Statements, https://tc39.es/ecma262/#sec-for-in-and-for-of-statements
internal sealed class ForInStatement : INode
{
    public ForInStatement(Identifier identifier, IExpression expression, INode iterationStatement)
    {
        Identifier = identifier;
        Expression = expression;
        IterationStatement = iterationStatement;
    }

    public Identifier Identifier { get; }
    public IExpression Expression { get; }
    public INode IterationStatement { get; }
}
