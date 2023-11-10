namespace JSS.Lib.AST;

// 15.2 Function Definitions, https://tc39.es/ecma262/#prod-FunctionDeclaration
internal sealed class FunctionDeclaration : INode
{
    // FIXME: Early errors for parameters having the same name, https://tc39.es/ecma262/#sec-function-definitions-static-semantics-early-errors
    public FunctionDeclaration(string identifier, List<Identifier> parameters, List<INode> body)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
    }

    // FIXME: 15.2.6 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-definitions-runtime-semantics-evaluation
    public void Execute() { }

    public string Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public IReadOnlyList<INode> Body { get; }
}
