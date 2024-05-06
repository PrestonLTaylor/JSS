namespace JSS.Lib.AST;

// 15.4 Method Definitions, https://tc39.es/ecma262/#prod-MethodDefinition
internal sealed class MethodDeclaration : INode
{
    public MethodDeclaration(string identifier, List<Identifier> parameters, StatementList body, bool isPrivate, bool isStrict)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
        IsPrivate = isPrivate;
        IsStrict = isStrict;
    }

    // FIXME: 15.4.5 Runtime Semantics: MethodDefinitionEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-methoddefinitionevaluation

    public string Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public StatementList Body { get; } 
    public bool IsPrivate { get; }
    public bool IsStrict { get; }
}
