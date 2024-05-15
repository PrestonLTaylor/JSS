namespace JSS.Lib.AST;

// 15.3 Arrow Function Definitions, https://tc39.es/ecma262/#sec-arrow-function-definitions
internal sealed class ArrowFunctionExpression : IExpression
{
    public ArrowFunctionExpression(List<Identifier> parameters, INode body, bool isStrict)
    {
        Parameters = parameters;
        Body = body;
        IsStrict = isStrict;
    }

    public List<Identifier> Parameters { get; }
    public INode Body { get; }
    public bool IsStrict { get; }
}
