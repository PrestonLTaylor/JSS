namespace JSS.Lib.AST;

// 15.3 Arrow Function Definitions, https://tc39.es/ecma262/#sec-arrow-function-definitions
internal sealed class ExpressionBody : INode
{
    public ExpressionBody(IExpression expression)
    {
        Expression = expression;
    }

    public IExpression Expression { get; }
}
