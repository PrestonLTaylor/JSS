namespace JSS.Lib.AST;

// 15.4 Method Definitions, https://tc39.es/ecma262/#prod-MethodDefinition
internal sealed class MethodDeclaration : INode
{
    public MethodDeclaration(string identifier, List<Identifier> parameters, List<INode> body, bool isPrivate)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
        IsPrivate = isPrivate;
    }

    // FIXME: 15.4.5 Runtime Semantics: MethodDefinitionEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-methoddefinitionevaluation
    public void Execute() { }

    public string Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public IReadOnlyList<INode> Body { get; } 
    public bool IsPrivate { get; }
}
